# Git Commands Shortcuts for UMGC-Capstone Project

## C# Terminal Shortcuts
- How to build the program
  dotnet build
  dotnet run

## Set Up and Configuration
- Clone the repository:
  git clone https://github.com/siddhutedla/UMGC-Capstone.git

- Navigate into the project directory:
  cd UMGC-Capstone

  or

  - Navigate into the project directory:
  cd ConcertFinder

## Basic Operations
- Check current status:
  git status

- Update local repository with changes from all branches:
  git fetch --all

## Branch Management
- List all branches:
  git branch -a

- Switch to an existing branch:
  git checkout [branch-name]

- Create a new branch and switch to it:
  git checkout -b [new-branch-name]

## Changes and Updates
- Add all changes to staging:
  git add .

- Commit staged changes:
  git commit -m "Describe changes here"

- Push changes from your branch to GitHub:
  git push origin [branch-name]

## Pulling Latest Changes
- Pull latest changes from the remote branch:
  git pull origin [branch-name]

## Merging Changes
- Merge changes from [branch-name] into the current branch:
  git merge [branch-name]

- Push merged changes to remote:
  git push origin [current-branch-name]

## Handling Merges and Conflicts
- After pulling changes, if there are conflicts, resolve them manually in the affected files, then:
  git add [resolved-file]
  git commit -m "Resolved merge conflicts"
  git push origin [current-branch-name]

Remember to replace placeholders like [branch-name], [new-branch-name], and [current-branch-name] with actual branch names relevant to your work.
