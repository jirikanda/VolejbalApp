import * as React from 'react';
import { connect } from 'react-redux';
import { Card, CardBody, CardTitle } from 'reactstrap';
import { bindActionCreators } from 'redux';

import { GridRowType } from '../../components/common/Treeview/GridInterfaces';
import { ApplicationState } from '../../store';
import * as CodebooksStore from '../../store/Codebooks';
import * as CustomAttributesStore from '../../store/CustomAttributes';
import * as ProductTypesStore from '../../store/ProductTypes';
import * as TopicsStore from '../../store/Topics';
import CustomAttributeEditForm, { FormName as CustomAttributeEditFormName } from './CustomAttributeEditForm';
import TopicEditForm from './TopicEditForm';
import { formValueSelector, getFormValues, change } from 'redux-form';
import { CustomAttributeType } from '../../apimodels/CustomAttributeValueStore';

type TopicsCustomAttributesEditContainerPropsType =
    CustomAttributesStore.CustomAttributeState
    & typeof CustomAttributesStore.actionCreators
    & TopicsStore.TopicState
    & typeof TopicsStore.actionCreators
    & ProductTypesStore.ProductTypesState
    & typeof ProductTypesStore.actionCreators
    & CodebooksStore.CodebooksState
    & { selectedRowType: GridRowType }

type TopicsCustomAttributesEditContainerProps =
    TopicsCustomAttributesEditContainerPropsType
    & { initialValues: TopicsCustomAttributesEditContainerPropsType, canSetMeasureUnit: boolean, customAttributeMeasureUnitId: number }

class TopicsCustomAttributesEditContainer extends React.Component<TopicsCustomAttributesEditContainerProps> {
    componentWillMount() {
        const {requestTopics, productType} = this.props;

        if (productType) {
            requestTopics(productType.id);
        }
    }

    componentWillReceiveProps(nextProps: TopicsCustomAttributesEditContainerProps) {
        const {requestTopics, requestTopic, productType, topic, topics} = this.props;

        if(nextProps.productType && (productType === null || nextProps.productType.id !== productType.id)) {
            requestTopics(nextProps.productType.id);
        }
    }

    render() {
        const { topicVM, selectedRowType, customAttributeVM } = this.props;

        const isTopic = topicVM && customAttributeVM == null;

        return <Card>
                <CardBody>
                {
                    (topicVM || customAttributeVM)
                        ? <>
                        <CardTitle>
                            Detail {isTopic ? `tématu - ${(topicVM && topicVM.name) ? topicVM.name : "nové téma"}` : `upřesňující podmínky - ${(customAttributeVM && customAttributeVM.name) ? customAttributeVM.name : "nová upřesňující podmínka"}`}
                        </CardTitle>
                        <Card>
                            <CardBody>
                                {
                                    isTopic ? <TopicEditForm {...this.props} /> : <CustomAttributeEditForm {...this.props} />
                                }
                            </CardBody>
                        </Card>
                        </>
                        : <CardTitle>Zatím nebylo vybráno žádné téma nebo upřesňující podmínka</CardTitle>
                }
                </CardBody>
            </Card>
    }
}

const mapStateToProps = (state: ApplicationState, props: TopicsCustomAttributesEditContainerPropsType) => {
    const customAttributeId = formValueSelector(CustomAttributeEditFormName)(state, "id") as number;
    const customAttributeType = formValueSelector(CustomAttributeEditFormName)(state, "type") as CustomAttributeType;
    const customAttributeMeasureUnitId = formValueSelector(CustomAttributeEditFormName)(state, "measureUnitId") as number;

    const canSetMeasureUnit = (customAttributeId == null) && (customAttributeType === CustomAttributeType.Integer);        

    return {
        topicVM: state.topics.topicVM,
        customAttributeVM: state.customAttributes.customAttributeVM,
        canSetMeasureUnit: canSetMeasureUnit,
        customAttributeMeasureUnitId: customAttributeMeasureUnitId,
        productType: state.productTypes.productType,
        measureUnits: state.codebooks.measureUnits,
        selectedRowType: props.selectedRowType
    }
};

const mapDispatchToProps = (dispatch, ownProps) => ({
    ...bindActionCreators(TopicsStore.actionCreators, dispatch),
    ...bindActionCreators(ProductTypesStore.actionCreators, dispatch),
    ...bindActionCreators(CustomAttributesStore.actionCreators, dispatch)
});

export default connect(mapStateToProps, mapDispatchToProps)(TopicsCustomAttributesEditContainer)