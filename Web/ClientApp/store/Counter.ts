import { Dispatch, Reducer } from 'redux';
import { delay } from 'redux-saga';
import { put, takeEvery, select } from 'redux-saga/effects';
// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface CounterState {
    count: number;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.

export enum TopicsActionTypes {
    IncrementCount = 'INCREMENT_COUNT',
    DecrementCount = 'DECREMENT_COUNT',
    IncrementCountAsync = 'INCREMENT_COUNT_ASYNC',
    DecrementCountAsync = 'DECREMENT_COUNT_ASYNC'
}

interface IncrementCountAction {
    type: TopicsActionTypes.IncrementCount;
    count: number;
}

interface DecrementCountAction {
    type: TopicsActionTypes.DecrementCount;
    count: number;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type ReducerKnownAction = IncrementCountAction | DecrementCountAction;

// Sagas
function* incrementAsync() {
    const state = yield select();   // just like getState()
    const counter: CounterState = state.counter;

    yield delay(1000)
    yield put({ type: TopicsActionTypes.IncrementCount, count: counter.count + 1 })
}

function* decrementAsync() {
    const state = yield select();   // just like getState()
    const counter: CounterState = state.counter;

    yield delay(1000)
    yield put({ type: TopicsActionTypes.DecrementCount, count: counter.count - 1 })
}

export function* watchCounterAsync() {
    yield takeEvery(TopicsActionTypes.IncrementCountAsync, incrementAsync)
    yield takeEvery(TopicsActionTypes.DecrementCountAsync, decrementAsync)
}

export const actionDispatchers = (dispatch: Dispatch<ReducerKnownAction>) => ({
    incrementAsync: () => { dispatch({ type: TopicsActionTypes.IncrementCountAsync }) },
    decrementAsync: () => { dispatch({ type: TopicsActionTypes.DecrementCountAsync }) }
})

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: CounterState = { count: 0 };

export const reducer: Reducer<CounterState> = (state: CounterState, action: ReducerKnownAction) => {
    switch (action.type) {
        case TopicsActionTypes.IncrementCount:
            return {
                ...state,
                count: action.count
            };
        case TopicsActionTypes.DecrementCount:
            return {
                ...state,
                count: action.count
            };
        default:
            const exhaustiveCheck: never = action;
    }

    // For unrecognized actions (or in cases where actions have no effect), must return the existing state
    //  (or default initial state if none was supplied
    return state || unloadedState;
};
