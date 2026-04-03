#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import math
from dataclasses import dataclass
from decimal import Decimal
from pathlib import Path
from typing import Iterable

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt


@dataclass(frozen=True)
class ThresholdSpec:
    raw_value: float
    suffix: str
    reached_col: str
    evals_col: str


def threshold_suffix(value: float) -> str:
    dec = Decimal(str(value))
    text = format(dec, "f").rstrip("0").rstrip(".")
    if not text:
        text = "0"
    return text.replace("-", "m").replace(".", "_")


def build_threshold_specs(thresholds: Iterable[float]) -> list[ThresholdSpec]:
    specs: list[ThresholdSpec] = []
    for value in thresholds:
        suffix = threshold_suffix(value)
        specs.append(
            ThresholdSpec(
                raw_value=float(value),
                suffix=suffix,
                reached_col=f"reached_{suffix}",
                evals_col=f"evals_to_{suffix}",
            )
        )
    return specs


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Анализ результатов benchmark-а для ГА и PSO."
    )
    parser.add_argument(
        "--input",
        required=True,
        help="Папка с manifest.json, runs.csv и history.csv",
    )
    parser.add_argument(
        "--output",
        default=None,
        help="Куда сохранять анализ. По умолчанию: <input>/analysis",
    )
    return parser.parse_args()


def load_manifest(path: Path) -> dict:
    if not path.exists():
        raise FileNotFoundError(f"Не найден manifest.json: {path}")
    with path.open("r", encoding="utf-8") as f:
        return json.load(f)


def load_csv(path: Path) -> pd.DataFrame:
    if not path.exists():
        raise FileNotFoundError(f"Не найден CSV: {path}")
    return pd.read_csv(path)


def coerce_bool_series(series: pd.Series) -> pd.Series:
    mapping = {
        "true": True,
        "false": False,
        "1": True,
        "0": False,
        True: True,
        False: False,
    }
    return series.map(lambda x: mapping.get(str(x).strip().lower(), np.nan))


def ensure_numeric(df: pd.DataFrame, columns: list[str]) -> None:
    for col in columns:
        if col in df.columns:
            df[col] = pd.to_numeric(df[col], errors="coerce")


def finite_mean(series: pd.Series) -> float:
    vals = series[np.isfinite(series)]
    return float(vals.mean()) if len(vals) else math.nan


def finite_median(series: pd.Series) -> float:
    vals = series[np.isfinite(series)]
    return float(vals.median()) if len(vals) else math.nan


def finite_std(series: pd.Series) -> float:
    vals = series[np.isfinite(series)]
    return float(vals.std(ddof=1)) if len(vals) > 1 else math.nan


def finite_min(series: pd.Series) -> float:
    vals = series[np.isfinite(series)]
    return float(vals.min()) if len(vals) else math.nan


def finite_max(series: pd.Series) -> float:
    vals = series[np.isfinite(series)]
    return float(vals.max()) if len(vals) else math.nan


def compute_summary(runs: pd.DataFrame, thresholds: list[ThresholdSpec]) -> pd.DataFrame:
    rows: list[dict] = []

    for case_id, group in runs.groupby("case_id", sort=True):
        row: dict[str, object] = {
            "case_id": case_id,
            "run_count": int(len(group)),
            "mean_final_best_fitness": finite_mean(group["final_best_fitness"]),
            "mean_final_best_error": finite_mean(group["final_best_error"]),
            "median_final_best_error": finite_median(group["final_best_error"]),
            "std_final_best_error": finite_std(group["final_best_error"]),
            "best_final_best_error": finite_min(group["final_best_error"]),
            "worst_final_best_error": finite_max(group["final_best_error"]),
            "mean_evaluation_count": finite_mean(group["evaluation_count"]),
            "mean_elapsed_ms": finite_mean(group["elapsed_ms"]),
            "mean_auc_log_error": finite_mean(group["auc_log_error"]),
        }

        status_counts = group["status"].value_counts(dropna=False)
        for status, count in status_counts.items():
            row[f"status_{status}"] = int(count)

        for spec in thresholds:
            reached = group[spec.reached_col]
            evals = group[spec.evals_col]

            reached_valid = reached.dropna()
            success_rate = float(reached_valid.mean()) if len(reached_valid) else math.nan

            successful_evals = evals[np.isfinite(evals)]
            row[f"success_rate_{spec.suffix}"] = success_rate
            row[f"mean_evals_to_{spec.suffix}"] = (
                float(successful_evals.mean()) if len(successful_evals) else math.nan
            )
            row[f"median_evals_to_{spec.suffix}"] = (
                float(successful_evals.median()) if len(successful_evals) else math.nan
            )

        rows.append(row)

    summary = pd.DataFrame(rows)
    if not summary.empty:
        summary = summary.sort_values("mean_final_best_error", kind="stable")
    return summary


def plot_convergence_by_iteration(
    history: pd.DataFrame,
    output_path: Path,
    epsilon: float,
) -> None:
    plt.figure(figsize=(11, 6))

    for case_id, group in history.groupby("case_id", sort=True):
        grouped = (
            group.groupby("iteration", as_index=False)["best_error"]
            .median()
            .sort_values("iteration")
        )

        y = np.maximum(grouped["best_error"].to_numpy(dtype=float), epsilon)
        plt.plot(grouped["iteration"], y, label=case_id)

    plt.yscale("log")
    plt.xlabel("Iteration")
    plt.ylabel("Median best error")
    plt.title("Сходимость по итерациям")
    plt.grid(True, alpha=0.3)
    plt.legend()
    plt.tight_layout()
    plt.savefig(output_path, dpi=160)
    plt.close()


def plot_convergence_by_evaluations(
    history: pd.DataFrame,
    output_path: Path,
    epsilon: float,
) -> None:
    plt.figure(figsize=(11, 6))

    for case_id, group in history.groupby("case_id", sort=True):
        grouped = (
            group.groupby("evaluation_count", as_index=False)["best_error"]
            .median()
            .sort_values("evaluation_count")
        )

        y = np.maximum(grouped["best_error"].to_numpy(dtype=float), epsilon)
        plt.plot(grouped["evaluation_count"], y, label=case_id)

    plt.yscale("log")
    plt.xlabel("Evaluation count")
    plt.ylabel("Median best error")
    plt.title("Сходимость по числу вычислений функции")
    plt.grid(True, alpha=0.3)
    plt.legend()
    plt.tight_layout()
    plt.savefig(output_path, dpi=160)
    plt.close()


def plot_final_error_boxplot(
    runs: pd.DataFrame,
    output_path: Path,
    epsilon: float,
) -> None:
    labels: list[str] = []
    values: list[np.ndarray] = []

    for case_id, group in runs.groupby("case_id", sort=True):
        err = group["final_best_error"].to_numpy(dtype=float)
        err = err[np.isfinite(err)]
        if len(err) == 0:
            continue
        labels.append(case_id)
        values.append(np.maximum(err, epsilon))

    plt.figure(figsize=(12, 6))
    plt.boxplot(values, labels=labels, showfliers=True)
    plt.yscale("log")
    plt.xticks(rotation=20, ha="right")
    plt.ylabel("Final best error")
    plt.title("Распределение финальной ошибки")
    plt.grid(True, axis="y", alpha=0.3)
    plt.tight_layout()
    plt.savefig(output_path, dpi=160)
    plt.close()


def plot_success_rates(
    summary: pd.DataFrame,
    thresholds: list[ThresholdSpec],
    output_path: Path,
) -> None:
    if summary.empty:
        return

    x = np.arange(len(summary))
    width = 0.8 / max(len(thresholds), 1)

    plt.figure(figsize=(12, 6))

    for i, spec in enumerate(thresholds):
        col = f"success_rate_{spec.suffix}"
        if col not in summary.columns:
            continue
        plt.bar(
            x + i * width - 0.4 + width / 2,
            summary[col].fillna(0.0).to_numpy(dtype=float),
            width=width,
            label=f"≤ {spec.raw_value:g}",
        )

    plt.xticks(x, summary["case_id"], rotation=20, ha="right")
    plt.ylim(0.0, 1.05)
    plt.ylabel("Success rate")
    plt.title("Доля успешных запусков по порогам ошибки")
    plt.grid(True, axis="y", alpha=0.3)
    plt.legend(title="Threshold")
    plt.tight_layout()
    plt.savefig(output_path, dpi=160)
    plt.close()


def plot_mean_evals_to_threshold(
    summary: pd.DataFrame,
    thresholds: list[ThresholdSpec],
    output_path: Path,
) -> None:
    if summary.empty:
        return

    x = np.arange(len(summary))
    width = 0.8 / max(len(thresholds), 1)

    plt.figure(figsize=(12, 6))

    for i, spec in enumerate(thresholds):
        col = f"mean_evals_to_{spec.suffix}"
        if col not in summary.columns:
            continue

        values = summary[col].to_numpy(dtype=float)
        plt.bar(
            x + i * width - 0.4 + width / 2,
            np.nan_to_num(values, nan=0.0),
            width=width,
            label=f"≤ {spec.raw_value:g}",
        )

    plt.xticks(x, summary["case_id"], rotation=20, ha="right")
    plt.ylabel("Mean evals to threshold")
    plt.title("Среднее число вычислений до достижения порога")
    plt.grid(True, axis="y", alpha=0.3)
    plt.legend(title="Threshold")
    plt.tight_layout()
    plt.savefig(output_path, dpi=160)
    plt.close()


def main() -> None:
    args = parse_args()

    input_dir = Path(args.input).resolve()
    output_dir = Path(args.output).resolve() if args.output else input_dir / "analysis"
    output_dir.mkdir(parents=True, exist_ok=True)

    manifest = load_manifest(input_dir / "manifest.json")
    runs = load_csv(input_dir / "runs.csv")
    history = load_csv(input_dir / "history.csv")

    thresholds_raw = manifest["metrics"]["thresholds"]
    epsilon = float(manifest["metrics"]["log_error_epsilon"])
    thresholds = build_threshold_specs(thresholds_raw)

    ensure_numeric(
        runs,
        [
            "seed",
            "iterations_completed",
            "evaluation_count",
            "elapsed_ms",
            "final_best_fitness",
            "final_best_error",
            "best_x",
            "best_y",
            "auc_log_error",
        ]
        + [spec.evals_col for spec in thresholds],
    )

    ensure_numeric(
        history,
        [
            "seed",
            "iteration",
            "evaluation_count",
            "best_fitness",
            "best_error",
            "best_x",
            "best_y",
        ],
    )

    for spec in thresholds:
        if spec.reached_col in runs.columns:
            runs[spec.reached_col] = coerce_bool_series(runs[spec.reached_col])
        if spec.evals_col not in runs.columns:
            runs[spec.evals_col] = np.nan

    summary = compute_summary(runs, thresholds)

    summary_path = output_dir / "summary_by_case.csv"
    summary.to_csv(summary_path, index=False)

    plot_convergence_by_iteration(
        history=history,
        output_path=output_dir / "convergence_by_iteration.png",
        epsilon=epsilon,
    )

    plot_convergence_by_evaluations(
        history=history,
        output_path=output_dir / "convergence_by_evaluations.png",
        epsilon=epsilon,
    )

    plot_final_error_boxplot(
        runs=runs,
        output_path=output_dir / "final_error_boxplot.png",
        epsilon=epsilon,
    )

    plot_success_rates(
        summary=summary,
        thresholds=thresholds,
        output_path=output_dir / "success_rates.png",
    )

    plot_mean_evals_to_threshold(
        summary=summary,
        thresholds=thresholds,
        output_path=output_dir / "mean_evals_to_threshold.png",
    )

    print(f"Готово. Анализ сохранён в: {output_dir}")


if __name__ == "__main__":
    main()