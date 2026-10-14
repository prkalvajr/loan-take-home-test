import type { Metadata } from "next";
import Link from "next/link";
import { Suspense } from "react";
import { ResultCard, resultStyles as styles } from "@/components/ResultCard";
import approvedStyles from "./approved.module.css";

export const metadata: Metadata = { title: "Application approved | Business Loans" };

type SearchParams = Promise<{ type?: string | string[]; id?: string | string[] }>;

export default function ApprovedPage({ searchParams }: { searchParams: SearchParams }) {
  // Reading searchParams is request-time data, so it renders inside a Suspense boundary.
  return (
    <Suspense fallback={null}>
      <ApprovedResult searchParams={searchParams} />
    </Suspense>
  );
}

async function ApprovedResult({ searchParams }: { searchParams: SearchParams }) {
  const { type, id } = await searchParams;
  const applicationId = typeof id === "string" ? id : undefined;
  const isUpdate = type === "updated";

  if (!applicationId || (type !== "new" && type !== "updated")) {
    return (
      <ResultCard tone="neutral">
        <div className={styles.heading}>
          <h1 className={styles.title}>No application to show</h1>
          <p className={styles.body}>This page shows the result right after you submit an application.</p>
        </div>
        <div>
          <Link href="/" className={styles.primaryLink}>
            Go to the application form
          </Link>
        </div>
      </ResultCard>
    );
  }

  return (
    <ResultCard tone="success">
      <div className={styles.heading}>
        <h1 className={styles.title}>{isUpdate ? "Your application was updated" : "Your application is approved"}</h1>
        <p className={styles.body}>
          {isUpdate
            ? "We found an existing application for this SSN and updated it with the details you just submitted. You still have one application with us."
            : "We received your application and saved it. Your details are now being sent for processing."}
        </p>
      </div>

      <dl className={approvedStyles.details}>
        <dt>Reference</dt>
        <dd className={approvedStyles.reference}>{applicationId}</dd>
        <dt>Status</dt>
        <dd>{isUpdate ? "Existing application updated" : "New application created"}</dd>
      </dl>

      <p className={styles.muted}>Keep the reference above if you need to contact us about this application.</p>

      <div>
        <Link href="/" className={styles.primaryLink}>
          Back to the application form
        </Link>
      </div>
    </ResultCard>
  );
}
