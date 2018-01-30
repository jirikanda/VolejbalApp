import * as React from "react";
import { connect } from 'react-redux';
import { MenuComponent } from '../components/MenuComponent';
import { ApplicationState } from '../store/index';
import * as ProfileStore from '../store/Profile';
import { UserState } from 'redux-oidc';
import { withRouter, RouteComponentProps } from "react-router-dom";

type MenuProps = UserState
    & RouteComponentProps<Object>
    & ProfileStore.ProfileState

class Menu extends React.PureComponent<MenuProps> {
    render() {
        return <MenuComponent {...this.props} />
    }
}

const mapStateToProps = (state: ApplicationState) => {
    return {
        ...state.oidcReducer,
        ...state.profile
    }
}

export default withRouter(connect(mapStateToProps)(Menu))