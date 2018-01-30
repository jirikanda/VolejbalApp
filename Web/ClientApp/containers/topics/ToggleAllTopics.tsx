import * as React from 'react';
import { connect } from 'react-redux';
import * as TopicsStore from '../../store/Topics';
import { ApplicationState } from '../../store';
import ToggleAll from '../../components/common/ToggleAll';

type ToggleAllTopicsProps =
    TopicsStore.TopicState
    & typeof TopicsStore.actionCreators

const ToggleAllTopics = (props: ToggleAllTopicsProps) => <ToggleAll isAnySelected={props.selectedTopics.length > 0} onSelectAll={props.selectAllTopics} onUnselectAll={props.unselectAllTopics} />

export default connect((state: ApplicationState) => state.topics, TopicsStore.actionCreators)(ToggleAllTopics)