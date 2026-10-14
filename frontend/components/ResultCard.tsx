import type { ReactNode } from "react";
import styles from "./ResultCard.module.css";

type Tone = "success" | "error" | "neutral";

const toneClass: Record<Tone, string> = {
  success: styles.iconSuccess,
  error: styles.iconError,
  neutral: styles.iconNeutral,
};

const icons: Record<Tone, ReactNode> = {
  success: <path d="M7 14.5 L12 19.5 L21 9" stroke="#1F5E4A" strokeWidth="2.6" fill="none" strokeLinecap="round" strokeLinejoin="round" />,
  error: (
    <>
      <path d="M14 7.5 V15.5" stroke="#9E2F14" strokeWidth="2.6" strokeLinecap="round" />
      <circle cx="14" cy="20" r="1.6" fill="#9E2F14" />
    </>
  ),
  neutral: (
    <>
      <circle cx="14" cy="14" r="9" stroke="#56615B" strokeWidth="2" fill="none" />
      <path d="M14 9.5 V14.5" stroke="#56615B" strokeWidth="2.2" strokeLinecap="round" />
      <circle cx="14" cy="18.5" r="1.3" fill="#56615B" />
    </>
  ),
};

/** Shared frame for the approved and denied pages: centered card with a status icon. */
export function ResultCard({ tone, children }: { tone: Tone; children: ReactNode }) {
  return (
    <main className={styles.main}>
      <div className={styles.card}>
        <div className={`${styles.icon} ${toneClass[tone]}`}>
          <svg aria-hidden="true" width="28" height="28" viewBox="0 0 28 28">
            {icons[tone]}
          </svg>
        </div>
        {children}
      </div>
    </main>
  );
}

export { styles as resultStyles };
