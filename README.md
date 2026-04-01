# Argus

A lightweight Windows service that watches for DWM crash loops and automatically kills suspect processes before Windows logs you out.

## Why

If you use tools that hook into DWM (like some Windhawk mods), a bad hook can cause DWM to crash loop, rendering Windows unusable. Argus sits in the background, detects when this is happening, and kills the offending processes before it gets that far.

## Installation

Run `install.bat` as administrator. That's it.

To uninstall, run `uninstall.bat` as administrator.

## Configuration

Edit `argus.json` (the one in the same folder as the exe). Argus will use defaults if the file is missing.

```json
{
    "RestartThreshold": 3,
    "RestartWindowSeconds": 10,
    "KillList": ["Windhawk"]
}
```

| Field                  | Description                                                | Default        |
| ---------------------- | ---------------------------------------------------------- | -------------- |
| `RestartThreshold`     | Number of DWM restarts within the window to trigger a kill | `3`            |
| `RestartWindowSeconds` | Rolling time window in seconds                             | `10`           |
| `KillList`             | Process names to kill when threshold is hit (no `.exe`)    | `["Windhawk"]` |

## Logging

Argus writes a log to `argus.log` in the same folder as the exe. Check this if you want to see what it has been doing.

## Notes

- Argus runs as SYSTEM so it has the privileges needed to kill protected processes
- Changes to `argus.json` require a service restart to take effect. You can use install.bat and uninstall.bat, or you can do `sc stop Argus && sc start Argus`.