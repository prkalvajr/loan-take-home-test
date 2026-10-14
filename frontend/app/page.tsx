import { ApplicationForm } from "@/components/ApplicationForm";
import styles from "./page.module.css";

export default function ApplyPage() {
  return (
    <main className={styles.main}>
      <h1 className={styles.title}>Apply for a business loan</h1>
      <p className={styles.lead}>Tell us about you and your company. You will see a decision as soon as you submit.</p>

      <div className={styles.layout}>
        <div className={styles.formColumn}>
          <ApplicationForm />
        </div>

        <aside className={styles.aside}>
          <div className={styles.card}>
            <h2 className={styles.cardTitle}>Before you start</h2>
            <ul className={styles.checklist}>
              <li>Your legal first and last name</li>
              <li>Your company name</li>
              <li>Business address in the US</li>
              <li>The amount you want to request</li>
              <li>Your Social Security Number</li>
            </ul>
          </div>
          <div className={styles.tintCard}>
            <h2 className={styles.cardTitle}>Already applied?</h2>
            <p className={styles.note}>
              Submitting again with the same SSN updates your existing application instead of creating a new one.
            </p>
          </div>
        </aside>
      </div>
    </main>
  );
}
