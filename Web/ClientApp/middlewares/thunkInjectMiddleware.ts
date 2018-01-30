const thunkInjectMiddleware = (...inject) => {
    return ({ dispatch, getState }) => {
        return next => action => {
            if (typeof action === 'function') {
                return action(dispatch, getState, ...inject)
            } else {
                return next(action)
            }
        }
    }
}

export default thunkInjectMiddleware