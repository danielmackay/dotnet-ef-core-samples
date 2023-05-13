# Table Per Type

## Simple DB Example

In the simple example, we have two entities:  `Animal` and `Pet` and each maps to a table in the DB.

Note that `Pet.Id` is both a PK and FK to `Animal`

```mermaid
erDiagram
    Animals {
        int Id
        string Name
        string Species
    }

    Pets {
        int Id
        string Name
        string FavouriteToy
        string Vet
    }

    Animals ||--|| Pets : is-a
```

## Complex DB Example

In the complex example, we have five entities: `Animal`, `Pet`, `FarmAnimal`, `Cat`, and `Dog`, each of which maps to a table in the DB.

```mermaid
erDiagram
    Animals {
        int Id
        string Name
    }

    Pets {
        int Id
        string Vet
    }

    FarmAnimals {
        int Id
        string Farm
    }

    Cats {
        int Id
        string EducationLevel
    }

    Dogs {
        int Id
        string FavouriteToy
    }

    Animals ||--|| Pets : is-a
    Animals ||--|| FarmAnimals : is-a
    Pets ||--|| Cats : is-a
    Pets ||--|| Dogs : is-a
```

## Guidance

TPT is not recommended unless you are constrained by external factors.

## Resources

- https://learn.microsoft.com/en-us/ef/core/modeling/inheritance#table-per-type-configuration
