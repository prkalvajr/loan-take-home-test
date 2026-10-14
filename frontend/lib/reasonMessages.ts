/**
 * Maps the backend's stable denial reason codes to the text shown on the denied page.
 * A new backend rule only needs one new entry here; unknown codes fall back to a generic message.
 */
export type ReasonMessage = {
  title: string;
  message: string;
  icon: "location" | "identity" | "generic";
};

export const REASON_MESSAGES: Record<string, ReasonMessage> = {
  STATE_NOT_ALLOWED: {
    title: "Business location",
    message: "We are not able to offer loans in New York at this time.",
    icon: "location",
  },
  SSN_BLACKLISTED: {
    title: "Identity check",
    message: "We are unable to approve an application for this SSN.",
    icon: "identity",
  },
};

export const FALLBACK_REASON: ReasonMessage = {
  title: "Eligibility",
  message: "We are unable to approve this application.",
  icon: "generic",
};

export function reasonMessage(code: string): ReasonMessage {
  return REASON_MESSAGES[code] ?? FALLBACK_REASON;
}
