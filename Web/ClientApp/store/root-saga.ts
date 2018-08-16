import { delay,  } from 'redux-saga'
import { put, takeEvery, all, call } from 'redux-saga/effects'
import { watchCounterAsync } from '../store/Counter'

export default function* rootSaga() {
    yield all([
        watchCounterAsync()
    ])
}