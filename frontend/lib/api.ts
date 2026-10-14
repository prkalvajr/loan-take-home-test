import type { ApplicationFormValues } from "./applicationSchema";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5080";

/** Body of POST /api/applications. */
export type SubmitApplicationRequest = {
  firstName: string;
  lastName: string;
  companyName: string;
  ssn: string;
  requestedAmount: number;
  address: {
    street: string;
    city: string;
    state: string;
    zipCode: string;
  };
};

type ApprovedResponse = {
  decision: "approved";
  applicationId: string;
  customerId: string;
  isReturningCustomer: boolean;
};

type DeniedResponse = {
  decision: "denied";
  reasons: string[];
};

export type SubmitResult =
  | { kind: "approved"; applicationId: string; isReturningCustomer: boolean }
  | { kind: "denied"; reasons: string[] }
  | { kind: "invalid"; fieldErrors: Partial<Record<keyof ApplicationFormValues, string>> }
  | { kind: "conflict" }
  | { kind: "error" };

export function toRequest(values: ApplicationFormValues): SubmitApplicationRequest {
  return {
    firstName: values.firstName.trim(),
    lastName: values.lastName.trim(),
    companyName: values.companyName.trim(),
    ssn: values.ssn.replace(/\D/g, ""),
    requestedAmount: Number(values.requestedAmount.replaceAll(",", "")),
    address: {
      street: values.street.trim(),
      city: values.city.trim(),
      state: values.state,
      zipCode: values.zipCode.trim(),
    },
  };
}

export async function submitApplication(values: ApplicationFormValues): Promise<SubmitResult> {
  let response: Response;
  try {
    response = await fetch(`${API_URL}/api/applications`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(toRequest(values)),
    });
  } catch {
    return { kind: "error" };
  }

  if (response.status === 200) {
    const body = (await response.json()) as ApprovedResponse | DeniedResponse;
    return body.decision === "approved"
      ? { kind: "approved", applicationId: body.applicationId, isReturningCustomer: body.isReturningCustomer }
      : { kind: "denied", reasons: body.reasons };
  }

  if (response.status === 400) {
    const problem = (await response.json().catch(() => null)) as { errors?: Record<string, string[]> } | null;
    return { kind: "invalid", fieldErrors: toFieldErrors(problem?.errors ?? {}) };
  }

  if (response.status === 409) return { kind: "conflict" };

  return { kind: "error" };
}

/** Maps ValidationProblemDetails keys like "Address.ZipCode" or "ssn" to form field names. */
function toFieldErrors(errors: Record<string, string[]>) {
  const fields: Partial<Record<keyof ApplicationFormValues, string>> = {};
  for (const [key, messages] of Object.entries(errors)) {
    const last = key.split(".").pop() ?? key;
    const field = (last.charAt(0).toLowerCase() + last.slice(1)) as keyof ApplicationFormValues;
    if (messages.length > 0) fields[field] = messages[0];
  }
  return fields;
}
