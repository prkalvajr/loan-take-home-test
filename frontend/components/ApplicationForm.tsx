"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { applicationSchema, emptyApplicationForm, type ApplicationFormValues } from "@/lib/applicationSchema";
import { submitApplication } from "@/lib/api";
import { formatAmount, formatSsn, sanitizeAmount } from "@/lib/formatters";
import { US_STATES } from "@/lib/usStates";
import { FormField } from "./FormField";
import styles from "./ApplicationForm.module.css";

type Banner = { title: string; message: string };

const SERVER_ERROR: Banner = {
  title: "We could not submit your application.",
  message: "Something went wrong on our side. Your answers are still here, so you can try again.",
};

const CONFLICT: Banner = {
  title: "This application was already being submitted.",
  message: "Please submit again. It will update your existing application.",
};

const INVALID: Banner = {
  title: "Some details need another look.",
  message: "Check the highlighted fields and submit again.",
};

export function ApplicationForm() {
  const router = useRouter();
  const [banner, setBanner] = useState<Banner | null>(null);
  const [redirecting, setRedirecting] = useState(false);
  const bannerRef = useRef<HTMLDivElement>(null);

  const {
    register,
    control,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<ApplicationFormValues>({
    resolver: zodResolver(applicationSchema),
    mode: "onTouched",
    defaultValues: emptyApplicationForm,
  });

  useEffect(() => {
    if (banner) bannerRef.current?.focus();
  }, [banner]);

  const onSubmit = async (values: ApplicationFormValues) => {
    setBanner(null);
    const result = await submitApplication(values);

    switch (result.kind) {
      case "approved": {
        setRedirecting(true);
        const params = new URLSearchParams({
          type: result.isReturningCustomer ? "updated" : "new",
          id: result.applicationId,
        });
        router.push(`/approved?${params}`);
        return;
      }
      case "denied": {
        setRedirecting(true);
        router.push(`/denied?${new URLSearchParams({ reasons: result.reasons.join(",") })}`);
        return;
      }
      case "invalid": {
        const entries = Object.entries(result.fieldErrors) as [keyof ApplicationFormValues, string][];
        entries.forEach(([field, message]) => setError(field, { message }));
        setBanner(INVALID);
        return;
      }
      case "conflict":
        setBanner(CONFLICT);
        return;
      default:
        setBanner(SERVER_ERROR);
    }
  };

  const busy = isSubmitting || redirecting;

  return (
    <>
      {banner && (
        <div ref={bannerRef} role="alert" tabIndex={-1} className={styles.banner}>
          <svg aria-hidden="true" width="20" height="20" viewBox="0 0 20 20" className={styles.bannerIcon}>
            <circle cx="10" cy="10" r="8.5" stroke="currentColor" strokeWidth="1.6" fill="none" />
            <path d="M10 5.5 V11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
            <circle cx="10" cy="14" r="1.1" fill="currentColor" />
          </svg>
          <div>
            <strong>{banner.title}</strong>
            {banner.message}
          </div>
        </div>
      )}

      <form className={styles.form} onSubmit={handleSubmit(onSubmit)} noValidate>
        <fieldset className={styles.fieldset}>
          <legend className={styles.legend}>About you</legend>
          <div className={styles.row}>
            <FormField id="firstName" label="First name" error={errors.firstName?.message} className={styles.half}>
              {(p) => <input {...p} type="text" autoComplete="given-name" placeholder="Jane" {...register("firstName")} />}
            </FormField>
            <FormField id="lastName" label="Last name" error={errors.lastName?.message} className={styles.half}>
              {(p) => <input {...p} type="text" autoComplete="family-name" placeholder="Doe" {...register("lastName")} />}
            </FormField>
          </div>
        </fieldset>

        <fieldset className={styles.fieldset}>
          <legend className={styles.legend}>Your company</legend>
          <FormField id="companyName" label="Company name" error={errors.companyName?.message}>
            {(p) => (
              <input {...p} type="text" autoComplete="organization" placeholder="Acme LLC" {...register("companyName")} />
            )}
          </FormField>
        </fieldset>

        <fieldset className={styles.fieldset}>
          <legend className={styles.legend}>Business address</legend>
          <div className={styles.stack}>
            <FormField id="street" label="Street address" error={errors.street?.message}>
              {(p) => (
                <input
                  {...p}
                  type="text"
                  autoComplete="address-line1"
                  placeholder="100 Main St, Suite 200"
                  {...register("street")}
                />
              )}
            </FormField>
            <div className={styles.row}>
              <FormField id="city" label="City" error={errors.city?.message} className={styles.city}>
                {(p) => <input {...p} type="text" autoComplete="address-level2" placeholder="Austin" {...register("city")} />}
              </FormField>
              <FormField id="state" label="State" error={errors.state?.message} className={styles.state}>
                {(p) => (
                  <select {...p} autoComplete="address-level1" {...register("state")}>
                    <option value="">Select a state</option>
                    {US_STATES.map((s) => (
                      <option key={s.code} value={s.code}>
                        {s.name}
                      </option>
                    ))}
                  </select>
                )}
              </FormField>
              <FormField id="zipCode" label="ZIP code" error={errors.zipCode?.message} className={styles.zip}>
                {(p) => (
                  <input
                    {...p}
                    type="text"
                    inputMode="numeric"
                    autoComplete="postal-code"
                    placeholder="73301"
                    {...register("zipCode")}
                  />
                )}
              </FormField>
            </div>
          </div>
        </fieldset>

        <fieldset className={styles.fieldset}>
          <legend className={styles.legend}>Loan</legend>
          <FormField
            id="requestedAmount"
            label="Requested amount"
            error={errors.requestedAmount?.message}
            hint="The full amount in US dollars."
            className={styles.narrow}
          >
            {(p) => (
              <Controller
                name="requestedAmount"
                control={control}
                render={({ field }) => (
                  <div className={styles.amountWrapper}>
                    <span aria-hidden="true" className={styles.amountPrefix}>
                      $
                    </span>
                    <input
                      {...p}
                      className={`${p.className} ${styles.amountInput}`}
                      type="text"
                      inputMode="decimal"
                      placeholder="25,000.00"
                      name={field.name}
                      ref={field.ref}
                      value={field.value}
                      onChange={(e) => field.onChange(sanitizeAmount(e.target.value))}
                      onBlur={() => {
                        field.onChange(formatAmount(field.value));
                        field.onBlur();
                      }}
                    />
                  </div>
                )}
              />
            )}
          </FormField>
        </fieldset>

        <fieldset className={styles.fieldset}>
          <legend className={styles.legend}>Identity</legend>
          <FormField
            id="ssn"
            label="Social Security Number"
            error={errors.ssn?.message}
            hint="Used to check eligibility and to find an existing application. We only ever show the last 4 digits."
            className={styles.narrow}
          >
            {(p) => (
              <Controller
                name="ssn"
                control={control}
                render={({ field }) => (
                  <input
                    {...p}
                    className={`${p.className} ${styles.ssnInput}`}
                    type="text"
                    inputMode="numeric"
                    autoComplete="off"
                    placeholder="XXX-XX-XXXX"
                    name={field.name}
                    ref={field.ref}
                    value={field.value}
                    onChange={(e) => field.onChange(formatSsn(e.target.value))}
                    onBlur={field.onBlur}
                  />
                )}
              />
            )}
          </FormField>
        </fieldset>

        <div className={styles.actions}>
          <button type="submit" className={styles.submit} disabled={busy}>
            {busy ? "Submitting..." : "Submit application"}
          </button>
          <span className={styles.confirm}>By submitting, you confirm the information is accurate.</span>
        </div>
      </form>
    </>
  );
}
