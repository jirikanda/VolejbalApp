import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../store';
import * as CounterStore from '../store/Counter';

type CounterProps =
    CounterStore.CounterState
    & typeof CounterStore.actionCreators

class Counter extends React.Component<CounterProps> {
    render() {
        return <>
            <p>Counter</p>
            <p>This is a simple example of a React component connected to Redux store with notifications for user by Toastr.</p>
            <p>Current count: <strong>{this.props.count}</strong></p>
            <button onClick={() => this.props.increment()}>Increment</button>
            <button onClick={() => this.props.decrement()}>Decrement</button>
        </>;
    }
}

// Wire up the React component to the Redux store
export default connect(
    (state: ApplicationState) => state.counter, // Selects which state properties are merged into the component's props
    CounterStore.actionCreators                 // Selects which action creators are merged into the component's props
)(Counter);