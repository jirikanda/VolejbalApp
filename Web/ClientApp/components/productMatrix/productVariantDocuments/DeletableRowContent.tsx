import * as React from 'react';

import { getReadDateFormat } from '../../../helpers/momentHelpers';
import { ProductVariantDocument } from '../../../store/ProductVariantDocuments';
import DeleteButton from './DeleteButton';

interface DeletableRowContentProps {
    productVariantDocument: ProductVariantDocument;
    deleteProductVariantDocuments: (productVariantDocument: ProductVariantDocument) => void;
}

const DeletableRowContent = (props: DeletableRowContentProps) => {
    const {productVariantDocument} = props;

    return <td>
        {
            productVariantDocument.validTo
                ? `${getReadDateFormat(productVariantDocument.validFrom)} - ${getReadDateFormat(productVariantDocument.validTo)}`
                : `Od: ${getReadDateFormat(productVariantDocument.validFrom)}`
        }
        <DeleteButton {...props} />
    </td>
}

export default DeletableRowContent