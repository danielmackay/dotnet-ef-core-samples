# Table Per Hierarchy

TPH is the default inheritance strategy for EF Core. 

## Simple DB Example

In the simple example we have two entities:  `Animal` and `Pet`.  However, in the DB we have only a single table with the union of fields between both.  The `Discriminator` field tells EF which row maps to which data type.

Both `Animal` and `Pet` are concrete types, which means we can use one table to represent both.

```mermaid
erDiagram
    Animals {
        int Id
        string Name
        string Species
        string Discriminator
        string FavouriteToy
        string Vet
    }
```

## Complex DB Example

In the complex example we have five entities: `Animal`, `Pet`, `FarmAnimal`, `Cat`, and `Dog`.  However, these only map to three tables in the DB.

`Animal` and `Pet` are both abstract classes, which means they are combined with the tables that inherit from them.

```mermaid
erDiagram
    Cat {
        int Id
        string EducationLevel
        string Name
        string Vet
    }
```

```mermaid
erDiagram
    Dog {
        int Id
        string FavouriteToy
        string Name
        string Vet
    }
```

```mermaid
erDiagram
    FarmAnimal {
        int Id
        string Farm
        string Name
    }
```

## Guidance

TPH is usually fine for most applications and is a good default for a wide range of scenarios.
