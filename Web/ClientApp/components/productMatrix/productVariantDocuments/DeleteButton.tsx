import * as React from 'react';
import { Button, Table  } from 'reactstrap';
import Modal from '../../common/Modal';
import { ProductVariantDocument } from '../../../store/ProductVariantDocuments';
import * as moment from 'moment';
import { getReadDateFormat } from '../../../helpers/momentHelpers';
import { Toastr } from '../../../store/commonActionCreators/Toaster';

interface DeleteButtonProps {
    productVariantDocument: ProductVariantDocument;
    deleteProductVariantDocuments: (productVariantDocument: ProductVariantDocument) => void;
}

const DeleteButton = ({productVariantDocument, deleteProductVariantDocuments}: DeleteButtonProps) => {
    const handleRemoveButtonClick = () => {
        Toastr.confirm({
            message: 'Opravdu chcete soubor nenávratně odstranit?',
            onOk: () => {
                deleteProductVariantDocuments(productVariantDocument);
            }
        });
    }

    return <button type="button"
            className="pointer pull-right remove-methodology-document"
            onClick={handleRemoveButtonClick} >
            <i className="fa fa-times" aria-hidden="true"></i>
        </button>
}

export default DeleteButton