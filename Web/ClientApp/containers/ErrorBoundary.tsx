import * as React from "react";
import ErrorPage from './pages/ErrorPage'
import { connect } from "react-redux";
import { ApplicationState } from "../store";
import * as ErrorsStore from "../store/Errors";
import { Redirect, withRouter, RouteComponentProps } from 'react-router';

type ErrorBoundaryProps = ErrorsStore.ErrorState
    & typeof ErrorsStore.actionCreators
    & RouteComponentProps<{}>

class ErrorBoundary extends React.Component<ErrorBoundaryProps> {
    componentWillReceiveProps(nextProps: ErrorBoundaryProps) {
        if (nextProps.location.pathname !== this.props.location.pathname) {
            this.props.clearError();
        }
    }

    componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
        this.props.trackError(this.props.location.pathname, error, errorInfo);
    }

    render() {
        let renderResult = this.props.children;

        if (this.props.error || this.props.applicationError) {
            renderResult = <ErrorPage {...this.props} />;
        }

        return renderResult;
    }
}

export default withRouter(connect((state: ApplicationState) => state.errors, ErrorsStore.actionCreators)(ErrorBoundary))