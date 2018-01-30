import * as React from 'react'
import { connect } from 'react-redux';
import { ApplicationState } from '../../../store/index'
import CellEditDialog from '../../../components/productMatrix/customAttributeEdit/CellEditDialog';
import { ProductVariant } from '../../../apimodels/ProductVariantsStore';
import { CustomAttribute } from '../../../apimodels/ProductMatrixStore';
import { Topic } from '../../../apimodels/TopicsStore';
import * as ProductMatrixStore from '../../../store/ProductMatrix';

interface CellEditDialogContainerPropsPublic {
    showDialog: boolean;
    closeDialog: () => void;
    productVariant: ProductVariant;
    customAttribute: CustomAttribute;
    ancestorTopicsAndMe: Topic[];
}

type CellEditDialogContainerProps = CellEditDialogContainerPropsPublic
    & ProductMatrixStore.ProductMatrixState
    & typeof ProductMatrixStore.actionCreators

class CellEditDialogContainer extends React.Component<CellEditDialogContainerProps> {

    closeDialogAndRefreshProductMatrix = () => {
        this.props.isQuickSelection ? this.props.requestCustomAttributesQuickSelection(this.props.selectedProductType.id) : this.props.requestCustomAttributes(this.props.selectedProductType.id);
        this.props.closeDialog();
    }

    render() {
        return <CellEditDialog {...this.props} closeDialog={() => this.closeDialogAndRefreshProductMatrix()} />
    }
}

export default connect((state: ApplicationState, props: CellEditDialogContainerPropsPublic) => state.productMatrix, ProductMatrixStore.actionCreators)(CellEditDialogContainer)