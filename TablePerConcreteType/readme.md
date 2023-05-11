# Table Per Concrete Type

## Simple DB Example

In the simple example, we have two entities:  `Animal` and `Pet` and each maps to a table in the DB.

Data is denormalized so each table contains the union of all properties for all types in the hierarchy.

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
        string Species
        string FavouriteToy
        string Vet
    }
```

## Complex DB Example

In the complex example, we have five entities: `Animal`, `Pet`, `FarmAnimal`, `Cat`, and `Dog`.  These map to three tables in the DB (one for each concrete class).

Data is denormalized so each table contains the union of all properties for all types in the hierarchy.

```mermaid
erDiagram
    FarmAnimals {
        int Id
        string Farm
        string Name
    }

    Cats {
        int Id
        string Name
        string Vet
        string EducationLevel
    }

    Dogs {
        int Id
        string Name
        string Vet
        string FavouriteToy
    }
```

## Guidance

TPC provides similar control to TPT but with additional performance benefits.  It is recommended that you use TPC over TPT.
