#define _POSIX_C_SOURCE 200809L

#include <errno.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>

int main(int argc, char *argv[])
{
    if (argc < 2 || argv[1][0] == '\0')
    {
        fputs("Usage: GitExtensions.ProcessGroupLauncher <command> [arguments...]\n", stderr);
        return 64;
    }

    if (setpgid(0, 0) == -1)
    {
        fprintf(stderr, "Unable to create a process group: %s\n", strerror(errno));
        return 71;
    }

    // The managed caller passes ProcessStartInfo's executable and argument vector directly.
    // No shell interprets this input, and execvp preserves the original argument boundaries.
    // flawfinder: ignore
    execvp(argv[1], &argv[1]);
    fprintf(stderr, "Unable to execute %s: %s\n", argv[1], strerror(errno));
    return 127;
}
