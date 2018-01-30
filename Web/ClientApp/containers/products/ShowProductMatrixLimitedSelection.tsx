import * as React from "react";
import { RouteComponentProps, NavLink } from "react-router-dom";
import { connect } from "react-redux";
import { ApplicationState } from "../../store";
import * as ProductMatrixStore from "../../store/ProductMatrix";
import * as ProductVariants from "../../store/ProductVariants";
import * as Topics from "../../store/Topics";
import { withRouter } from 'react-router';

type ShowProductMatrixLimitedSelectionProps =
    ProductMatrixStore.ProductMatrixState
    & typeof ProductMatrixStore.actionCreators
    & RouteComponentProps<{ segmentId: number; productTypeId: number }>

class ShowProductMatrixLimitedSelection extends React.Component<ShowProductMatrixLimitedSelectionProps> {
    render() {
        const { requestCustomAttributesQuickSelection, match } = this.props;

        return <>
            <NavLink
                to={`/segments/${match.params.segmentId}/product-types/${match.params.productTypeId}/product-matrix`}
                className="btn btn-primary"
                onClick={e => requestCustomAttributesQuickSelection(match.params.productTypeId)}>
                Rychlý přehled
                </NavLink>
        </>
    }
}

export default withRouter(connect(
    (state: ApplicationState, props: RouteComponentProps<{ segmentId: number; productTypeId: number }>) =>
        state.productMatrix,
    ProductMatrixStore.actionCreators
)(ShowProductMatrixLimitedSelection));
