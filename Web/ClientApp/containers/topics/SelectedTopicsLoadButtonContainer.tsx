import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store/index';
import * as TopicStore from '../../store/Topics';
import SelectedTopicsLoadButton from '../../components/topics/SelectedTopicsLoadButton';

type SelectedTopicsLoadButtonContainerProps =
    TopicStore.TopicState
    & typeof TopicStore.actionCreators
    & { selectedProductTypeId: number }

class SelectedTopicsLoadButtonContainer extends React.Component<SelectedTopicsLoadButtonContainerProps> {
    render() {
        return <SelectedTopicsLoadButton {...this.props} />
    }
}

export default connect((state: ApplicationState, props: { selectedProductTypeId: number }) => state.topics, TopicStore.actionCreators)(SelectedTopicsLoadButtonContainer)