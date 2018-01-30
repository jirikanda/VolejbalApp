import * as React from 'react';
import { connect } from 'react-redux';

import { ProductType } from '../../../apimodels/SegmentsStore';
import { Topic, CustomAttributeVM } from '../../../apimodels/TopicsStore';
import Grid from '../../../components/common/Treeview/Grid';
import { GridConfig, GridData, GridRow, GridRowType } from '../../../components/common/Treeview/GridInterfaces';
import { ApplicationState } from '../../../store/index';
import * as TopicStore from '../../../store/Topics';
import * as CustomAttributesStore from '../../../store/CustomAttributes';
import { FormName as CustomAttributeEditFormName } from '../CustomAttributeEditForm';
import { FormName as TopicEditFormName } from '../TopicEditForm';
import { TopicVM } from 'ClientApp/apimodels/Topics';
import { getFlattenNodes } from '../../../helpers/hierarchy';
import CurrentCustomAttributeValueForm from 'ClientApp/components/productMatrix/customAttributeEdit/CurrentCustomAttributeValueForm';

type TopicsTreeviewProps =
    TopicStore.TopicState
    & typeof TopicStore.actionCreators
    & typeof CustomAttributesStore.actionCreators
    & CustomAttributesStore.CustomAttributeState
    & { productType: ProductType }

const recursive = (topic: Topic, topicVM: TopicVM, customAttributeVM: CustomAttributeVM): GridRow => {
    const gridRow: GridRow = {
        id: topic.id,
        cells: [{
            name: topic.name,
            classNames: [!topic.isActive ? "text-muted" : null]
        }],
        children: [],
        rowType: GridRowType.Topic,
        isSelected: topicVM && (customAttributeVM == undefined) && (topic.id === topicVM.id)
    };

    topic.children.forEach((child, index) => {
        if(child.children.length > 0 || child.customAttributes.length > 0) {
            gridRow.children.push(recursive(child, topicVM, customAttributeVM))
            return;
        }

        gridRow.children.push({
            id: child.id,
            cells: [{
                name: child.name,
                classNames: [!child.isActive ? "text-muted" : null]
            }],
            children: [],
            rowType: GridRowType.Topic,
            isSelected: (topicVM && child.id === topicVM.id)
        });
    });

    topic.customAttributes.forEach((customAttribute, index) => {
        gridRow.children.push({
            id: customAttribute.id,
            cells: [{name: customAttribute.name}],
            children: [],
            rowType: GridRowType.CustomAttribute,
            isSelected: customAttributeVM && (customAttribute.id === customAttributeVM.id)
        });
    });

    return gridRow;
}

class TopicsTreeview extends React.Component<TopicsTreeviewProps> {

    render() {
        const { topics, requestTopic, requestCustomAttribute, productType, topicVM, customAttributeVM, clearCustomAttribute } = this.props;

        const flattenTopics = getFlattenNodes(topics);

        const gridConfig: GridConfig = {
            columns: [
                { columnName: "topics", displayName: "" }
            ],
            onRowClick: (itemId: number, rowType: GridRowType) => {
                if (rowType === GridRowType.Topic) {
                    clearCustomAttribute();
                    requestTopic(TopicEditFormName, productType.id, itemId);
                } else {
                    let topicId;
                    if (this.props.topicVM) {
                        topicId = this.props.topicVM.id;
                    } else {
                        topicId = flattenTopics.find(topic => topic.customAttributes.some(customAttribute => customAttribute.id === itemId)).id
                    }
                    
                    requestTopic(TopicEditFormName, productType.id, topicId);
                    requestCustomAttribute(CustomAttributeEditFormName, productType.id, itemId);
                }
            }
        }
        const gridData: GridData = {
            rows: topics.map((topic) => recursive(topic, topicVM, customAttributeVM))
        };

        return <>{
            gridData.rows.length > 0
                ? <Grid gridConfig={gridConfig} data={gridData} />
                : "Žádná témata nejsou dostupná"
        }</>
    }
}

export default connect((state: ApplicationState) => ({ ...state.topics, ...state.customAttributes, productType: state.productTypes.productType }),
({...TopicStore.actionCreators, ...CustomAttributesStore.actionCreators}))(TopicsTreeview);