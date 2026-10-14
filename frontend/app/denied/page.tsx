import type { Metadata } from "next";
import Link from "next/link";
import { Suspense } from "react";
import { ResultCard, resultStyles as styles } from "@/components/ResultCard";
import { reasonMessage, type ReasonMessage } from "@/lib/reasonMessages";
import deniedStyles from "./denied.module.css";

export const metadata: Metadata = { title: "Application denied | Business Loans" };

type SearchParams = Promise<{ reasons?: string | string[] }>;

export default function DeniedPage({ searchParams }: { searchParams: SearchParams }) {
  // Reading searchParams is request-time data, so it renders inside a Suspense boundary.
  return (
    <Suspense fallback={null}>
      <DeniedResult searchParams={searchParams} />
    </Suspense>
  );
}

async function DeniedResult({ searchParams }: { searchParams: SearchParams }) {
  const { reasons } = await searchParams;
  const codes = [...new Set((typeof reasons === "string" ? reasons : "").split(",").filter(Boolean))];

  return (
    <ResultCard tone="error">
      <div className={styles.heading}>
        <h1 className={styles.title}>We can&apos;t approve this application</h1>
        <p className={styles.body}>
          {codes.length > 0 ? "Your application did not meet our requirements. Here is why:" : "Your application did not meet our requirements."}
        </p>
      </div>

      {codes.length > 0 && (
        <ul className={deniedStyles.reasons}>
          {codes.map((code) => {
            const reason = reasonMessage(code);
            return (
              <li key={code} className={deniedStyles.reason}>
                <ReasonIcon icon={reason.icon} />
                <div className={deniedStyles.reasonText}>
                  <strong>{reason.title}</strong>
                  <span>{reason.message}</span>
                </div>
              </li>
            );
          })}
        </ul>
      )}

      <div className={deniedStyles.next}>
        <h2>What you can do</h2>
        <p>If any of your details were entered incorrectly, go back and correct them. Nothing was saved from this submission.</p>
      </div>

      <div>
        <Link href="/" className={styles.secondaryLink}>
          Back to the application
        </Link>
      </div>
    </ResultCard>
  );
}

function ReasonIcon({ icon }: { icon: ReasonMessage["icon"] }) {
  const stroke = { stroke: "#9E2F14", fill: "none" } as const;
  return (
    <svg aria-hidden="true" width="20" height="20" viewBox="0 0 20 20" className={deniedStyles.reasonIcon}>
      {icon === "location" && (
        <>
          <path d="M10 18 C10 18 4 12.2 4 8 A6 6 0 0 1 16 8 C16 12.2 10 18 10 18 Z" {...stroke} strokeWidth="1.6" strokeLinejoin="round" />
          <circle cx="10" cy="8" r="2" {...stroke} strokeWidth="1.6" />
        </>
      )}
      {icon === "identity" && (
        <>
          <rect x="2.5" y="4.5" width="15" height="11" rx="2" {...stroke} strokeWidth="1.6" />
          <circle cx="7.5" cy="10" r="1.8" {...stroke} strokeWidth="1.4" />
          <path d="M11.5 8.5 H15 M11.5 11.5 H14" {...stroke} strokeWidth="1.4" strokeLinecap="round" />
        </>
      )}
      {icon === "generic" && (
        <>
          <circle cx="10" cy="10" r="7.5" {...stroke} strokeWidth="1.6" />
          <path d="M6.5 13.5 L13.5 6.5" {...stroke} strokeWidth="1.6" strokeLinecap="round" />
        </>
      )}
    </svg>
  );
}
