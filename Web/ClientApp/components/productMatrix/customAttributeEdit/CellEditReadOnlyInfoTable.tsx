import * as React from 'react'
import TopicPath from '../../../components/common/TopicPath';
import { Table } from 'reactstrap';
import { ProductVariant } from '../../../apimodels/ProductVariantsStore';
import { CustomAttributeVM, CustomAttributeType } from '../../../apimodels/CustomAttributeValueStore';
import { Topic } from '../../../apimodels/TopicsStore';
import { getCustomAttributeTypeText } from '../../../helpers/customAttributeValues';

interface CellEditReadOnlyInfoTableProps {
    productVariant: ProductVariant;
    customAttribute: CustomAttributeVM;
    ancestorTopicsAndMe: Topic[];
}

const CellEditReadOnlyInfoTable = (props: CellEditReadOnlyInfoTableProps) => (
    <Table bordered size="sm">
        <tbody>
            <tr>
                <th>Upřesňující podmínka</th>
                <td>{props.customAttribute.name}</td>
            </tr>
            <tr>
                <th>Produkt</th>
                <td>{props.productVariant.name}</td>
            </tr>
            <tr>
                <th>Téma</th>
                <td><TopicPath topics={props.ancestorTopicsAndMe} /></td>
            </tr>
            <tr>
                <th>Datový typ odpovědi</th>
                <td>{getCustomAttributeTypeText(props.customAttribute.symbol)}</td>
            </tr>
        </tbody>
    </Table>)

export default CellEditReadOnlyInfoTable