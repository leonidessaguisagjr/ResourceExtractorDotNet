#!/usr/bin/env python3
"""
Script for generating some TestStrings#.restext files full of dummy data.
"""

import random
import string


def main():
    output_filename = "TestStrings{0}.txt"
    num_lines = 50
    chars_per_line = 70
    num_files = 5
    for i in range(1, num_files + 1):
        with open(output_filename.format(i), mode='w') as file_obj:
            for current_line in range(1, num_lines + 1):
                tmp = [random.choice(string.ascii_letters)
                       for x in range(chars_per_line)]
                file_obj.write(
                    "stringID{0}={1}\n".format(
                        current_line, "".join(tmp)))


if __name__ == "__main__":
    main()
