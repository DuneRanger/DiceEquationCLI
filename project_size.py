#!/usr/bin/env python3
"""
Calculate the total size of all .cs files in a directory, up to a specific depth.

Depth 0 = only files directly in the root directory
Depth 1 = root directory + one level of subdirectories
etc.
"""

import os
import argparse


def get_cs_files_size(root_dir, max_depth):
    """
    Walk the directory tree up to max_depth and sum the size of all .cs files.

    Args:
        root_dir (str): The directory to start searching from.
        max_depth (int): Maximum depth to descend into (0 = root only).

    Returns:
        tuple: (total_size_in_bytes, file_count)
    """
    total_size = 0
    file_count = 0
    root_dir = os.path.abspath(root_dir)
    root_depth = root_dir.rstrip(os.sep).count(os.sep)

    for dirpath, dirnames, filenames in os.walk(root_dir):
        current_depth = dirpath.rstrip(os.sep).count(os.sep) - root_depth

        if current_depth >= max_depth:
            # Prevent os.walk from descending further
            dirnames[:] = []

        for filename in filenames:
            if filename.lower().endswith(".cs"):
                filepath = os.path.join(dirpath, filename)
                try:
                    size = os.path.getsize(filepath)
                    total_size += size
                    file_count += 1
                except OSError as e:
                    print(f"Warning: could not read {filepath}: {e}")

    return total_size, file_count


def format_size(num_bytes):
    """Convert bytes into a human-readable string."""
    for unit in ["B", "KB", "MB", "GB", "TB"]:
        if num_bytes < 1024:
            return f"{num_bytes:.2f} {unit}"
        num_bytes /= 1024
    return f"{num_bytes:.2f} PB"


def main():
    parser = argparse.ArgumentParser(
        description="Calculate total size of .cs files up to a given depth."
    )
    parser.add_argument("directory", help="Root directory to scan")
    parser.add_argument(
        "-d", "--depth",
        type=int,
        default=0,
        help="Max depth to search (0 = only root dir, default: 0)"
    )
    args = parser.parse_args()

    if not os.path.isdir(args.directory):
        print(f"Error: '{args.directory}' is not a valid directory.")
        return

    total_size, file_count = get_cs_files_size(args.directory, args.depth)

    print(f"Directory: {os.path.abspath(args.directory)}")
    print(f"Max depth: {args.depth}")
    print(f"Files found: {file_count}")
    print(f"Total size: {format_size(total_size)} ({total_size} bytes)")


if __name__ == "__main__":
    main()
