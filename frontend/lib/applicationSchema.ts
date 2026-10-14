import { z } from "zod";
import { US_STATE_CODES } from "./usStates";

/**
 * Client-side validation, for UX only. The API validates the same rules again
 * and the domain value objects enforce them as invariants.
 */
const required = (label: string, max: number) =>
  z
    .string()
    .trim()
    .min(1, `Enter your ${label}.`)
    .max(max, `Use ${max} characters or fewer.`);

export const applicationSchema = z.object({
  firstName: required("first name", 100),
  lastName: required("last name", 100),
  companyName: required("company name", 200),
  street: required("street address", 200),
  city: required("city", 200),
  state: z
    .string()
    .refine((v) => (US_STATE_CODES as readonly string[]).includes(v), "Select a state."),
  zipCode: z
    .string()
    .trim()
    .regex(/^\d{5}(-\d{4})?$/, "Enter a 5-digit ZIP, like 73301."),
  requestedAmount: z
    .string()
    .trim()
    .refine((v) => {
      const plain = v.replaceAll(",", "");
      return /^\d+(\.\d{1,2})?$/.test(plain) && Number(plain) > 0;
    }, "Enter an amount greater than $0."),
  ssn: z
    .string()
    .trim()
    .regex(/^\d{3}-?\d{2}-?\d{4}$/, "Enter a 9-digit SSN, like 123-45-6789."),
});

export type ApplicationFormValues = z.infer<typeof applicationSchema>;

export const emptyApplicationForm: ApplicationFormValues = {
  firstName: "",
  lastName: "",
  companyName: "",
  street: "",
  city: "",
  state: "",
  zipCode: "",
  requestedAmount: "",
  ssn: "",
};
