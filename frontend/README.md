# Frontend

Next.js (App Router) app with three routes:

| Route       | Purpose |
| ----------- | ------- |
| `/`         | Application form. Validated with zod + react-hook-form, posts to `POST /api/applications`. |
| `/approved` | `?type=new\|updated&id=<applicationId>`. Shows "approved" or "updated" and the reference. |
| `/denied`   | `?reasons=STATE_NOT_ALLOWED,SSN_BLACKLISTED`. One message per reason code (`lib/reasonMessages.ts`). |

The result pages only receive IDs and reason codes in the URL, never the SSN or name.

```bash
npm install
npm run dev     # http://localhost:3000
npm run lint
npm run build
```

Set `NEXT_PUBLIC_API_URL` to point at the API (default `http://localhost:5080`).
