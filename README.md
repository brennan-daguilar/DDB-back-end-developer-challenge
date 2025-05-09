# DDB Back End Developer Challenge

### Overview

This is a sample service for managing player characters Hit Points, built for the DnD Beyond Backend Developer
Challenge.

## Decisions and Assumptions

### Microservices

From the challenge description, the goal is to manage character hit points, not the characters themselves. I've decided to consider that the character details are managed by another service.

This decision adds complication and potentially additional latency.  In a real product, I would consider whether separating hit points from the character details is worth the additional complication.  Some things to consider:
1. How frequently do active hit points and character details change and how often do changes to one affect the other?
2. How does this affect response latency?
3. Can character data be cached? Is cache invalidation required?
4. Is there an advantage to scaling the hit points service independently?
5. Does the team/organization structure benefit from smaller limited purpose services?

For the purposes of this challenge, I've included a simple Character Service to simulate retrieving character data externally.

### Response Data

For API responses, I considered some likely consumers of the API such as Maps or Sigil and what data would be useful for displaying feedback, animations, and combat logs to a user.

### Data Storage

For simplicity, data is stored in an sqlite database.  A key-value database like DynamoDB, with high scalability and low latency, may better fit storing character hit point states.

### Caching

Character details are cached using an in memory store.  In a real product this could be replaced with a proper distributed cache through dependency injection.

Having accurate character detail is important to properly applying damage or healing.  If character details are cached, there should be a mechanism to invalidate the cache or subscribe to updates when a character's details are updated.  In this case, character details never change so this has been omitted.

### Authentication and Authorization

Both authentication and authorization are omitted for simplicity but in a proper product would be included.  A proper implementation would need to consider whether this service would be publicly accessible or if it's an internal service that can rely on it's consumers to ensure proper authentication and/or authorization.


## API Endpoints

[OpenAPI Spec](openapi-v1.json)

### Deal Damage

- `POST /characters/{characterId}/damage`

    #### Request Body

    ```json
    {
      "amount": 10,
      "type": "bludgeoning"
    }
    ```

    #### Example Response

    ```json
    {
      "characterId": "briv",
      "before": {
        "hitpoints": 25,
        "temporaryHitpoints": 2
      },
      "after": {
        "hitpoints": 23,
        "temporaryHitpoints": 0
      },
      "totalDamage": 4,
      "resistanceEffect": "Resisted"
    }
    ```

### Heal

- `POST /characters/{characterId}/heal`
    
    #### Request Body
    
    ```json
    {
      "amount": 10
    }
    ```
    
    #### Example Response
    
    ```json
    {
      "characterId": "briv",
      "before": {
        "hitpoints": 23,
        "temporaryHitpoints": 0
      },
      "after": {
        "hitpoints": 25,
        "temporaryHitpoints": 0
      },
      "healed": 2
    }
    ```

### Add Temporary Hit Points

- `POST /characters/{characterId}/temporaryhitpoints`

    #### Request Body
    
    ```json
    {
      "amount": 10
    }
    ```
    
    #### Example Response
    
    ```json
    {
      "characterId": "briv",
      "before": {
        "hitpoints": 21,
        "temporaryHitpoints": 1
      },
      "after": {
        "hitpoints": 21,
        "temporaryHitpoints": 10
      },
      "gained": 9
    }
    ```

