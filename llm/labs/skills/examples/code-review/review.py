import subprocess
import sys
import json


def run_review(filepath: str) -> dict:
    """대상 파일에 대해 정적 분석을 실행하고 결과를 반환합니다."""
    results = {
        "filepath": filepath,
        "issues": [],
        "summary": ""
    }

    # pylint 실행
    try:
        proc = subprocess.run(
            ["pylint", filepath, "--output-format=json"],
            capture_output=True, text=True
        )
        if proc.stdout:
            pylint_issues = json.loads(proc.stdout)
            for issue in pylint_issues:
                results["issues"].append({
                    "tool": "pylint",
                    "line": issue.get("line"),
                    "message": issue.get("message"),
                    "severity": issue.get("type")
                })
    except FileNotFoundError:
        results["issues"].append({
            "tool": "pylint",
            "line": None,
            "message": "pylint이 설치되어 있지 않습니다",
            "severity": "info"
        })

    results["summary"] = f"정적 분석 완료: {len(results['issues'])}건 발견"
    return results


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python review.py <filepath>")
        sys.exit(1)

    result = run_review(sys.argv[1])
    print(json.dumps(result, ensure_ascii=False, indent=2))