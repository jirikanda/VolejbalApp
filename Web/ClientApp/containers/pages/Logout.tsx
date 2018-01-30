import * as React from 'react';
import { UserState } from 'redux-oidc';
import { RouteComponentProps, withRouter } from 'react-router';
import LogoutContainer from '../../containers/Logout'

type LogoutProps = UserState & RouteComponentProps<Object>

export class Logout extends React.Component<LogoutProps> {
    render() {
        return <LogoutContainer />
    }
}

export default withRouter(Logout)