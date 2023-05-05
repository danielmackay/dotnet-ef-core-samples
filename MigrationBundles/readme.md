# Migration Bundles

Migration Bundles allow you to generate a stand-alone executable that can upgrade a database.

## Use Cases

- Upgrade DB during DevOps pipelines, to avoid issues with multiple instances of the application trying to upgrade the database at the same time.
- Improves start-up time of application, by not having to run migrations on start-up.

## Usage

Run `ApplyBundleToLocal.ps1` or `ApplyBundleToEnvironment.ps1`.
