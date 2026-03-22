import os
import sys
import subprocess
import platform
from pathlib import Path
import argparse

def get_unity_path(custom_path=None):
    if custom_path:
        return custom_path

    system = platform.system()
    if system == "Windows":
        return r"C:\Program Files\Unity\Hub\Editor\6000.2.7f2\Editor\Unity.exe"
    elif system == "Linux":
        return f"{os.environ['HOME']}/Unity/Hub/Editor/6000.2.7f2/Editor/Unity"
    elif system == "Darwin":
        return "/Applications/Unity/Hub/Editor/6000.2.7f2/Unity.app/Contents/MacOS/Unity"
    else:
        raise Exception(f"Unsupported OS: {system}")
    
def build_project(target, output_path, project_path, build_number, version_path, unity_path):
    ver = version_path.read_text().strip()
    fullVer = f"{ver}.build-{build_number}"

    cmd = [
        unity_path,
        "-quit", "-batchmode", "-nographics", "-disableassemblyupdater",
        "-accept-apiupdate", "-disableburst-compilation",
        "-projectPath", project_path,
        "-executeMethod", "RIGPR.Editor.BuildManager.BuildProject",
        "-buildVersion", fullVer,
        "-buildTarget", target,
        "-outputPath", output_path,
        "-scriptingBackend", args.scripting_backend
    ]

    print(f"Building Unity Project:\n{' '.join(cmd)}")

    process = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
    for line in iter(process.stdout.readline, ''):
        print(line, end='')
    process.stdout.close()
    return_code = process.wait()

    if return_code != 0:
        print(f"\nBuild failed with exit code {return_code}")
        sys.exit(return_code)
    else:
        print(f"\nBuild completed successfully: {output_path}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Build Toy Soldiers for a specified target")
    parser.add_argument("--target", choices=["Win64", "Linux64", "OSXUniversal"], required=True)
    parser.add_argument("--scripting-backend", choices=["mono", "il2cpp"], default="il2cpp", required=True)
    parser.add_argument("--output", help="Output path for the build")
    parser.add_argument("--build-number", default="0", help="CI/CD build number")
    parser.add_argument("--unity-path", help="Optional custom path to Unity executable")
    args = parser.parse_args()

    SCRIPT_DIR = Path(__file__).resolve().parent
    REPO_ROOT = SCRIPT_DIR.parent
    version_path = REPO_ROOT / "version.txt"
    project_path = str(REPO_ROOT)

    if args.output:
        output = args.output
    else:
        platform_dir = args.target.lower().replace("64", "")
        ext = "exe" if args.target == "Win64" else "x86_64"
        output = REPO_ROOT / "Builds" / platform_dir / f"ToySoldiers.{ext}"

    unity_path = get_unity_path(args.unity_path)
    build_game(args.target, str(output), project_path, args.build_number, version_path, unity_path)