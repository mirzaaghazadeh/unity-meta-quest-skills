"""Relay between Claude Code and the `metavr` MCP server that repairs one protocol bug.

metavr 1.8.0.17.10 advertises MCP protocol 2026-07-28, which Claude Code 2.1.301 uses, but its list
results (tools/list, ...) omit the `ttlMs` and `cacheScope` fields that protocol version requires.
Claude Code then reports "Connected · tools fetch failed". This adds the two fields with no-caching
values (ttlMs 0, cacheScope "private") to any result that carries the 2026-07-28 `resultType` field
but lacks them. Every other message passes through unchanged. Python standard library only.

Usage (Claude Code):
    claude mcp add metavr -- python3 -I /path/to/metavr_mcp_relay.py ~/.metavr/bin/metavr mcp server --update
Remove the relay once `claude mcp get metavr` shows "Connected" without it.

Part of unity-meta-quest-skills (MIT), skill unity-quest-dev-setup. Found and tested on 2026-10-16.
"""
import json
import subprocess
import sys
import threading


def repair(line):
    try:
        message = json.loads(line)
    except ValueError:
        return line
    result = message.get("result") if isinstance(message, dict) else None
    if not isinstance(result, dict) or "resultType" not in result:
        return line
    if "ttlMs" in result and "cacheScope" in result:
        return line
    result.setdefault("ttlMs", 0)
    result.setdefault("cacheScope", "private")
    return json.dumps(message) + "\n"


def main():
    if len(sys.argv) < 2:
        sys.exit("usage: metavr_mcp_relay.py <metavr command and args...>")
    server = subprocess.Popen(sys.argv[1:], stdin=subprocess.PIPE, stdout=subprocess.PIPE, text=True, bufsize=1)

    def forward_requests():
        for line in sys.stdin:
            server.stdin.write(line)
            server.stdin.flush()
        server.stdin.close()

    threading.Thread(target=forward_requests, daemon=True).start()
    for line in server.stdout:
        sys.stdout.write(repair(line))
        sys.stdout.flush()
    sys.exit(server.wait())


if __name__ == "__main__":
    main()
