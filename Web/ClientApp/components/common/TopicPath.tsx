import * as React from 'react';
import { Topic } from '../../apimodels/TopicsStore';

interface TopicPathProps {
    topics: Topic[];
}

const TopicPath = (props: TopicPathProps) =>
    <span>{props.topics.map(topic => topic.name).join(' / ')}</span>

export default TopicPath;