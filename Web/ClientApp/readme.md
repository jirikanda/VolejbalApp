# BrokerTrustMetodikyOnline client application
Project "Web" is only the bearer of client application implemented in this folder (~/ClientApp). The purpose of client application is to serve application to browser to render application data provided by WebApi project.

## Used patterns & technologies
- Typescript
- React
- Redux
- Reactstrap
- React Router
- Reactstrap (Bootstrap 4)
- ~~ASP.NET Core Prerendering~~ (turned off)

## Redux
#### Stores (or modules)
Aplication has single store represented by combination of smaller stores into one and only one.
Stores are units wrapping main parts of redux pattern.
- state
- action
- action creator
  - helper function with more logic (HTTP requests, etc.)
  - place where is needed to write code what cannot be part of reducer (impure code/function)
- reducer
  - must be pure function 

New store (actionCreators and actionTypes respectively) should be added to routes as loader wrapper should know about what is async action and what is not.

#### Actions
- actions has required property for type of action called "type"
  - these types are written in upper-case and underscore-delimited format i.e. 'REQUEST_SEGMENT', 'SELECT_TOPICS', etc.
- every action which does 
  - request to WebApi via HTTP should begin with 'REQUEST_*'
  - receive from WebApi via HTTP should begin with 'RECEIVE_*'
  - 

//TBD