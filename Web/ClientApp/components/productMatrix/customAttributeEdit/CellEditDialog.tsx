import * as React from 'react'
import Modal from '../../common/Modal';
import { ProductVariant } from '../../../apimodels/ProductVariantsStore';
import { CustomAttribute } from '../../../apimodels/ProductMatrixStore';
import { Topic } from '../../../apimodels/TopicsStore';
import TopicPath from '../../../components/common/TopicPath';
import CellEditReadOnlyInfoTable from './CellEditReadOnlyInfoTable';
import CellEditForm from '../../../containers/products/customAttributeEdit/CellEditForm';

interface CellEditDialogProps {
    showDialog: boolean;
    closeDialog: () => void;
    productVariant: ProductVariant;
    customAttribute: CustomAttribute;
    ancestorTopicsAndMe: Topic[];
}

class CellEditDialog extends React.Component<CellEditDialogProps> {
    render() {
        const { showDialog, closeDialog, customAttribute, productVariant, ancestorTopicsAndMe } = this.props;

        return <Modal isOpen={showDialog} title={customAttribute.name} dialogSize="lg" handleDialogClose={closeDialog}>
            {/*customAttribute.id} {productVariant.id*/}
            <CellEditForm productVariant={productVariant} customAttribute={customAttribute} ancestorTopicsAndMe={ancestorTopicsAndMe} />
        </Modal>
    }
}

export default CellEditDialog;