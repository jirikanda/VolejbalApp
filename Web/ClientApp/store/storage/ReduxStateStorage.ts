{/**
 *
to persist whole/partial application state in session use this peace of code in boot-client.tsx

let initialState = window["initialReduxState"] as ApplicationState;

const persistedState = loadState();

if (persistedState) {
    initialState = persistedState;
}

const store = configureStore(history, superagentInstance, initialState);

store.subscribe(() => {
    saveState({
        productMatrix: store.getState().productMatrix
    });
})

 */}

export const loadState = () => {
    const serializedState = sessionStorage.getItem('state');
    return JSON.parse(serializedState);
}

export const saveState = (state: Object) => {
    const serializedState = JSON.stringify(state);
    sessionStorage.setItem('state', serializedState);
}