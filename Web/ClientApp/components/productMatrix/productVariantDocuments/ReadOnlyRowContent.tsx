import * as React from 'react';
import { Button, Table  } from 'reactstrap';
import Modal from '../../common/Modal';
import { ProductVariantDocument } from '../../../store/ProductVariantDocuments';
import * as moment from 'moment';
import { getReadDateFormat } from '../../../helpers/momentHelpers';

interface ReadOnlyRowContentProps {
    productVariantDocument: ProductVariantDocument;
}

const ReadOnlyRowContent = ({productVariantDocument}: ReadOnlyRowContentProps) =>
    <td>
        {
            productVariantDocument.validTo
                ? `${getReadDateFormat(productVariantDocument.validFrom)} - ${getReadDateFormat(productVariantDocument.validTo)}`
                : `Od: ${getReadDateFormat(productVariantDocument.validFrom)}`
        }
    </td>

export default ReadOnlyRowContent