import { Action, Reducer } from 'redux';
import { delay } from 'redux-saga'
import { put, takeEvery, all } from 'redux-saga/effects'
import { Dispatch } from 'redux'
// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface CounterState {
    count: number;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.

interface IncrementCountAction { type: 'INCREMENT_COUNT' }
interface DecrementCountAction { type: 'DECREMENT_COUNT' }

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = IncrementCountAction | DecrementCountAction;

function* incrementAsync() {
    yield delay(1000)
    yield put({ type: 'INCREMENT_COUNT' })
}

function* decrementAsync() {
    yield delay(1000)
    yield put({ type: 'DECREMENT_COUNT' })
}

export function* watchCounterAsync() {
    yield takeEvery('INCREMENT_ASYNC', incrementAsync)
    yield takeEvery('DECREMENT_ASYNC', decrementAsync)
}

export const actionDispatchers = (dispatch: Dispatch<KnownAction>) => ({
    incrementAsync: () => { dispatch({ type: 'INCREMENT_ASYNC' }) },
    decrementAsync: () => { dispatch({ type: 'DECREMENT_ASYNC' }) }
})

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

export const reducer: Reducer<CounterState> = (state: CounterState, action: KnownAction) => {
    switch (action.type) {
        case 'INCREMENT_COUNT':
            return { count: state.count + 1 };
        case 'DECREMENT_COUNT':
            return { count: state.count - 1 };
        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    // For unrecognized actions (or in cases where actions have no effect), must return the existing state
    //  (or default initial state if none was supplied)
    return state || { count: 0 };
};
