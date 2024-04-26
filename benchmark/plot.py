#! /usr/bin/env nix-shell
#! nix-shell -i python3 -p python311Packages.matplotlib python311Packages.pandas

"""This program shows parametrized `hyperfine` benchmark results as an
errorbar plot.
Adapted from https://github.com/sharkdp/hyperfine/blob/e8ff88dad130d4b4bc2362be92aa8dfedc35074a/scripts/plot_parametrized.py

The figure in the report is generated with the command:
```sh
./plot.py results/result_linear.json results/result_par.json --exp-x --titles Linear,Parallel
```
"""

import argparse
import json
import matplotlib.pyplot as plt
import sys

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("file", help="JSON file with benchmark results", nargs="+")
parser.add_argument(
    "--parameter-name",
    metavar="name",
    type=str,
    help="Deprecated; parameter names are now inferred from benchmark files",
)
parser.add_argument(
    "--log-x", help="Use a logarithmic x (parameter) axis", action="store_true"
)
parser.add_argument(
    "--log-time", help="Use a logarithmic time axis", action="store_true"
)
parser.add_argument(
    "--exp-x", help="Use an exponential x (parameter) axis", action="store_true"
)
parser.add_argument(
    "--titles", help="Comma-separated list of titles for the plot legend"
)
parser.add_argument(
    "-o", "--output", help="Save image to the given filename."
)

args = parser.parse_args()
if args.parameter_name is not None:
    sys.stderr.write(
        "warning: --parameter-name is deprecated; names are inferred from "
        "benchmark results\n"
    )


def die(msg):
    sys.stderr.write("fatal: %s\n" % (msg,))
    sys.exit(1)


def extract_parameters(results):
    """Return `(parameter_name: str, parameter_values: List[float])`."""
    if not results:
        die("no benchmark data to plot")
    (names, values) = zip(*(unique_parameter(b) for b in results))
    names = frozenset(names)
    if len(names) != 1:
        die(
            "benchmarks must all have the same parameter name, but found: %s"
            % sorted(names)
        )
    return (next(iter(names)), list(values))


def unique_parameter(benchmark):
    """Return the unique parameter `(name: str, value: float)`, or die."""
    params_dict = benchmark.get("parameters", {})
    if not params_dict:
        die("benchmarks must have exactly one parameter, but found none")
    if len(params_dict) > 1:
        die(
            "benchmarks must have exactly one parameter, but found multiple: %s"
            % sorted(params_dict)
        )
    [(name, value)] = params_dict.items()
    return (name, float(value))


parameter_name = None

for filename in args.file:
    with open(filename) as f:
        results = json.load(f)["results"]

    (this_parameter_name, parameter_values) = extract_parameters(results)
    if parameter_name is not None and this_parameter_name != parameter_name:
        die(
            "files must all have the same parameter name, but found %r vs. %r"
            % (parameter_name, this_parameter_name)
        )
    parameter_name = this_parameter_name

    times_mean = [b["mean"] for b in results]
    times_stddev = [b["stddev"] for b in results]

    # Adjust for compile time
    times_mean = [x - times_mean[0] for x in times_mean]
    times_stddev = [x - times_stddev[0] for x in times_stddev]

    if args.exp_x:
        parameter_values = [2 ** x * 1000 if x != 0 else 0 for x in parameter_values]

    plt.errorbar(x=parameter_values, y=times_mean, yerr=times_stddev, capsize=2)

plt.xlabel(parameter_name)
plt.ylabel("Time [s]")

if args.log_time:
    plt.yscale("log")
else:
    plt.ylim(0, None)

if args.log_x:
    plt.xscale("log")

if args.titles:
    plt.legend(args.titles.split(","))

if args.output:
    plt.savefig(args.output)
else:
    plt.show()
