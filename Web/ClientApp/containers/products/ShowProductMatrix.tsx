import * as React from "react";
import { RouteComponentProps, NavLink } from "react-router-dom";
import { connect } from "react-redux";
import { ApplicationState } from "../../store";
import * as ProductMatrixStore from "../../store/ProductMatrix";
import * as ProductVariants from "../../store/ProductVariants";
import * as Topics from "../../store/Topics";
import { withRouter } from 'react-router';

type ShowProductMatrixProps =
    ProductMatrixStore.ProductMatrixState
    & typeof ProductMatrixStore.actionCreators
    & RouteComponentProps<{ segmentId: number; productTypeId: number }>

class ShowProductMatrix extends React.Component<ShowProductMatrixProps> {
    render() {
        const { requestCustomAttributes, match } = this.props;

        return <div className="text-center">
                <NavLink
                to={`/segments/${match.params.segmentId}/product-types/${match.params.productTypeId}/product-matrix`}
                    className="btn btn-primary"
                    onClick={e => requestCustomAttributes(match.params.productTypeId)}>
                    Zobrazit
                </NavLink>
            </div>
    }
}

export default withRouter(connect(
    (state: ApplicationState, props: RouteComponentProps<{ segmentId: number; productTypeId: number }>) =>
        state.productMatrix,
    ProductMatrixStore.actionCreators
)(ShowProductMatrix));
