/* tslint:disable */
import * as React from 'react';
import { connect } from 'react-redux';
import Spinner from './Spinner';
import { withRouter, RouteComponentProps } from 'react-router';

const equal = require('deep-equal');

const deepIncludes = (array, item) => array.some(el => equal(el, item));

const shallowDesymbolize = (obj) => {
    if (obj instanceof Array) {
        return obj.map((el) => shallowDesymbolize(el));
    } else if (obj instanceof Object) {
        const res = {};
        Object.getOwnPropertySymbols(obj).forEach((key) => {
            const desymbolizedKey = ('__dsym__').concat(String(key));
            res[desymbolizedKey] = obj[key];
        });
        return res;
    }
    return obj;
}

interface LoaderProps {
    activeRequests: string[] | Object;
    dispatch: (action) => void;
    waitingOnStore: boolean;
}

export const activeRequests = (state = [], action) => {
    const newState = state.slice();

    // regex that tests for an API action string starting with REQUEST_ 
    const requestRegExp = new RegExp(/^REQUEST_[A-Z_]+$/g);
    // regex that tests for a API action string starting with RECEIVE_
    const receiveRegExp = new RegExp(/^RECEIVE_[A-Z_]+$/g);
    // regex that tests for a API action string starting with RECEIVE_ and ends with _ERROR
    const errorRegExp = new RegExp(/^ERROR_[A-Z_]+$/g);

    // if a REQUEST_ comes in, add it to the activeRequests list
    if (requestRegExp.test(action.type)) {
        newState.push(action.type);
    }

    // if a RECEIVE_ comes in, delete its corresponding REQUEST_
    if (receiveRegExp.test(action.type)) {
        const splittedActionType = action.type.split('_').filter((item, index) => index > 0) as string[];
        const requesteType = 'REQUEST_' + splittedActionType.join('_');
        const deletedIndex = state.indexOf(requesteType);

        if (deletedIndex !== -1) {
            newState.splice(deletedIndex, 1);
        }
    } else if (errorRegExp.test(action.type)) { // if a ERROR_ comes in, delete all REQUEST_
        newState.splice(0, newState.length);
    }

    return newState;
}

const loaderWrapperFactory = (actionsList, requestStates, stateInjector?) => {
    return (WrappedComponent) => {
        class Loader extends React.Component<LoaderProps & RouteComponentProps<{}>> {
            currentRequests;
            needsDispatch: boolean;

            constructor(props) {
                super(props);

                this.currentRequests = [];
                this.needsDispatch = true;
            }

            render() {
                const { activeRequests, dispatch } = this.props;

                let requestsBusy = false;
                // monitor given request states
                if (this.needsDispatch) {
                    // The case when there are no active requests yet (initial render)
                    // requestsBusy = true;
                } else if (activeRequests instanceof Array) {
                    requestsBusy = requestStates.some(state => activeRequests.some(a => a === state));
                } else if (activeRequests instanceof Object) {
                    requestsBusy = requestStates.some(state => Object.keys(activeRequests).some(a => a === state));
                } else {
                    console.warn('Loader: did not receive a valid requestStates object: ', requestStates);
                    return false;
                }

                // call actions, but throttle if repeating
                actionsList.forEach(action => {
                    if (!deepIncludes(shallowDesymbolize(this.currentRequests),
                        shallowDesymbolize(action))) {
                        this.currentRequests.push(action);
                        dispatch(action);
                        this.needsDispatch = false;
                    }
                });

                return (
                    <WrappedComponent {...this.props}>
                        {requestsBusy && <Spinner />}
                        {this.props.children}
                    </WrappedComponent>
                );                
            }
        }

        return withRouter(connect(stateInjector)(Loader) as any); // TODO: technical debt: get rid off this "as any" is not that easy at thought
    };
}

export default loaderWrapperFactory