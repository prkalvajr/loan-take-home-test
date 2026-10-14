import Link from "next/link";
import styles from "./SiteHeader.module.css";

export function SiteHeader() {
  return (
    <header className={styles.header}>
      <div className={styles.inner}>
        <Link href="/" className={styles.brand}>
          <svg aria-hidden="true" width="28" height="28" viewBox="0 0 28 28">
            <rect width="28" height="28" rx="6" fill="#1F5E4A" />
            <path
              d="M8 18 L12 14 L15 16 L20 10"
              stroke="#FFFFFF"
              strokeWidth="2"
              fill="none"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
          Business Loans
        </Link>
      </div>
    </header>
  );
}
