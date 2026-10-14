import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  cacheComponents: true,
  partialPrefetching: true,
  // Self-contained server output for the Docker image.
  output: "standalone",
};

export default nextConfig;
