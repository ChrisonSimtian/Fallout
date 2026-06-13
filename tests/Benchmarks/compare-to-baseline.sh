#!/usr/bin/env bash
#
# Perf regression gate. Compares a fresh BenchmarkDotNet JSON report against a committed baseline and fails
# (exit 1) if any benchmark's mean time OR allocations regressed beyond the threshold.
#
# Usage: compare-to-baseline.sh <fresh-report.json> <baseline.json> [threshold-percent]
#   <fresh-report.json>  a BDN *-report-full-compressed.json (from `dotnet run -c Release ... -- --exporters json`)
#   <baseline.json>      tests/Benchmarks/baselines/*.baseline.json (committed)
#   threshold-percent    allowed regression before failing (default 25 — generous, to absorb runner noise)
#
# NOTE: baseline numbers are machine-specific. Run this only against a baseline captured on the SAME
# environment (the CI runner). Refresh the baseline via the benchmarks workflow's refresh mode.
#
# No `set -e`: the script manages its own exit code via the counters below, and uses `[ cond ] && act`
# idioms that legitimately return non-zero when the condition is false.
set -uo pipefail

report="${1:?fresh report json required}"
baseline="${2:?baseline json required}"
threshold="${3:-25}"

# Flatten the fresh report to "fullname<TAB>meanNs<TAB>allocBytes".
fresh="$(jq -r '.Benchmarks[] | "\(.FullName)\t\(.Statistics.Mean)\t\(.Memory.BytesAllocatedPerOperation)"' "$report")"

regressions=0
missing=0
echo "Perf gate (threshold: +${threshold}% over baseline)"
echo "--------------------------------------------------------------------------"

while IFS=$'\t' read -r name base_mean base_alloc; do
  [ -z "$name" ] && continue
  line="$(grep -F "$name"$'\t' <<<"$fresh" || true)"
  if [ -z "$line" ]; then
    echo "MISSING  $name — in baseline but not in the fresh report"
    missing=$((missing + 1))
    continue
  fi
  cur_mean="$(cut -f2 <<<"$line")"
  cur_alloc="$(cut -f3 <<<"$line")"

  # ratios as percentages over baseline (awk for float math)
  read -r mean_pct alloc_pct verdict < <(awk -v bm="$base_mean" -v ba="$base_alloc" -v cm="$cur_mean" -v ca="$cur_alloc" -v t="$threshold" '
    BEGIN {
      mp = (bm > 0) ? (cm - bm) / bm * 100 : 0
      ap = (ba > 0) ? (ca - ba) / ba * 100 : 0
      v = (mp > t || ap > t) ? "REGRESSED" : "ok"
      printf "%.1f %.1f %s", mp, ap, v
    }')

  printf "%-9s %s  (time %+.1f%%, alloc %+.1f%%)\n" "$verdict" "$name" "$mean_pct" "$alloc_pct"
  [ "$verdict" = "REGRESSED" ] && regressions=$((regressions + 1))
done < <(jq -r '.benchmarks | to_entries[] | "\(.key)\t\(.value.meanNs)\t\(.value.allocatedBytes)"' "$baseline")

echo "--------------------------------------------------------------------------"
if [ "$missing" -gt 0 ]; then
  echo "FAIL: $missing baseline benchmark(s) missing from the report."; exit 1
fi
if [ "$regressions" -gt 0 ]; then
  echo "FAIL: $regressions benchmark(s) regressed beyond +${threshold}%."; exit 1
fi
echo "PASS: no benchmark regressed beyond +${threshold}%."
