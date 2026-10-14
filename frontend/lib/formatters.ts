/** Formats digits as XXX-XX-XXXX while the user types. */
export function formatSsn(value: string): string {
  const digits = value.replace(/\D/g, "").slice(0, 9);
  if (digits.length <= 3) return digits;
  if (digits.length <= 5) return `${digits.slice(0, 3)}-${digits.slice(3)}`;
  return `${digits.slice(0, 3)}-${digits.slice(3, 5)}-${digits.slice(5)}`;
}

/** Keeps only characters that can be part of an amount. */
export function sanitizeAmount(value: string): string {
  return value.replace(/[^\d.,]/g, "");
}

/** Adds thousands separators on blur, like 25000.5 -> 25,000.50. Leaves invalid input as typed. */
export function formatAmount(value: string): string {
  const plain = value.replaceAll(",", "");
  if (!/^\d+(\.\d{1,2})?$/.test(plain)) return value;
  return Number(plain).toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}
