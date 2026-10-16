#!/usr/bin/env python3
"""Summarize Unity test result files (NUnit3 XML from the Unity Test Framework, or JUnit XML).

Usage:
  python3 -I summarize_test_results.py test-results.xml [more.xml ...] [--json]
      [--max-failures 20] [--stack-lines 3]

Exit code: 0 = every test passed, 1 = at least one test failed, 2 = a file was unreadable
or contained no test cases. Standard library only. Run it with -I so nothing in the current
directory shadows a stdlib module.
"""
import argparse
import json
import sys
import xml.etree.ElementTree as ET

MAX_MESSAGE = 600


def _text(node, path):
    found = node.find(path)
    return (found.text or "").strip() if found is not None and found.text else ""


def _trim(text, limit=MAX_MESSAGE):
    text = text.strip()
    return text if len(text) <= limit else text[:limit] + " ..."


def _stack(text, lines):
    return [line.strip() for line in text.splitlines() if line.strip()][:lines]


def parse_nunit(root, stack_lines):
    cases = list(root.iter("test-case"))
    failures = []
    for case in cases:
        if case.get("result") == "Failed":
            failures.append({
                "name": case.get("fullname") or case.get("name") or "?",
                "message": _trim(_text(case, "failure/message")),
                "stack": _stack(_text(case, "failure/stack-trace"), stack_lines),
                "duration": case.get("duration"),
            })
    count = lambda result: sum(1 for c in cases if c.get("result") == result)
    return {
        "format": "nunit3",
        "result": root.get("result") or ("Failed" if failures else "Passed"),
        "total": len(cases),
        "passed": count("Passed"),
        "failed": count("Failed"),
        "skipped": count("Skipped"),
        "inconclusive": count("Inconclusive"),
        "duration": root.get("duration"),
        "failures": failures,
    }


def parse_junit(root, stack_lines):
    cases = list(root.iter("testcase"))
    failures, skipped = [], 0
    for case in cases:
        if case.find("skipped") is not None:
            skipped += 1
            continue
        bad = case.find("failure")
        if bad is None:
            bad = case.find("error")
        if bad is not None:
            name = ".".join(p for p in (case.get("classname"), case.get("name")) if p)
            failures.append({
                "name": name or "?",
                "message": _trim(bad.get("message") or ""),
                "stack": _stack(bad.text or "", stack_lines),
                "duration": case.get("time"),
            })
    return {
        "format": "junit",
        "result": "Failed" if failures else "Passed",
        "total": len(cases),
        "passed": len(cases) - len(failures) - skipped,
        "failed": len(failures),
        "skipped": skipped,
        "inconclusive": 0,
        "duration": root.get("time"),
        "failures": failures,
    }


def summarize(path, stack_lines):
    try:
        root = ET.parse(path).getroot()
    except (OSError, ET.ParseError) as exc:
        return {"file": path, "error": "could not read: %s" % exc}
    if root.tag == "test-run" or root.find(".//test-case") is not None:
        summary = parse_nunit(root, stack_lines)
    elif root.tag in ("testsuites", "testsuite"):
        summary = parse_junit(root, stack_lines)
    else:
        return {"file": path, "error": "unrecognised root element <%s>" % root.tag}
    summary["file"] = path
    if summary["total"] == 0:
        summary["error"] = "no test cases found (did the filter match nothing, or did the run abort?)"
    return summary


def print_human(summary, max_failures):
    if summary.get("error") and "total" not in summary:
        print("%s: ERROR %s" % (summary["file"], summary["error"]))
        return
    print("%s [%s]: %s - total %d, passed %d, failed %d, skipped %d, inconclusive %d, duration %ss" % (
        summary["file"], summary["format"], summary["result"], summary["total"], summary["passed"],
        summary["failed"], summary["skipped"], summary["inconclusive"], summary.get("duration") or "?"))
    if summary.get("error"):
        print("  WARNING: " + summary["error"])
    for failure in summary["failures"][:max_failures]:
        print("  FAIL %s" % failure["name"])
        if failure["message"]:
            for line in failure["message"].splitlines():
                print("       " + line)
        for line in failure["stack"]:
            print("       | " + line)
    hidden = len(summary["failures"]) - max_failures
    if hidden > 0:
        print("  ... %d more failure(s); use --max-failures or --json" % hidden)


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("files", nargs="+")
    parser.add_argument("--json", action="store_true", help="print one JSON document")
    parser.add_argument("--max-failures", type=int, default=20)
    parser.add_argument("--stack-lines", type=int, default=3)
    args = parser.parse_args(argv)

    summaries = [summarize(path, args.stack_lines) for path in args.files]
    if args.json:
        print(json.dumps(summaries, indent=2))
    else:
        for summary in summaries:
            print_human(summary, args.max_failures)

    if any(s.get("error") for s in summaries):
        return 2
    if any(s["failed"] for s in summaries):
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
