#!/bin/bash

# Build the mdbook
mdbook build

# Checkout the gh-pages branch
git checkout gh-pages

# If gh-pages doesn't exist, create it
if [ $? -ne 0 ]; then
    git checkout -b gh-pages
fi

# Remove all files in the current directory except .git
find . -maxdepth 1 ! -name '.git' ! -name '.' -exec rm -rf {} \;

# Copy the contents of the book/html directory to the current directory
cp -r book/* .

# Add all changes
git add --all .

# Commit changes
git commit -m "Deployed updates to GitHub Pages"

# Push changes
git push origin gh-pages

# Switch back to your main branch (assuming it's named main)
git checkout main
