import * as React from 'react';
import { connect } from 'react-redux';
import { NavLink } from 'react-router-dom'
import { ApplicationState } from '../../store/index';
import * as ProductMatrixStore from '../../store/ProductMatrix';
import { Button } from 'reactstrap';
import { withRouter, RouteComponentProps } from 'react-router';
import { goBack, go } from 'react-router-redux';

type GoToConfigurationPageButtonProps =
    RouteComponentProps<{}> &
    { isVisible: boolean }

class GoToConfigurationPageButton extends React.Component<GoToConfigurationPageButtonProps> {
    render() {
        return <>{this.props.isVisible &&
            <NavLink className="btn btn-primary" to={location.pathname.slice(0, location.pathname.lastIndexOf("/product-matrix"))}>Zpět na výběr témat</NavLink>
        }</>
    }
}

export default GoToConfigurationPageButton