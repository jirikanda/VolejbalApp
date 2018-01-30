import { SearchTopicsItem } from "../store/Topics";
import { getFlattenNodes, getAncestorsAndMe } from "./hierarchy";
import { Topic } from "../apimodels/TopicsStore";

/**
 * returns array of SearchTopicsItems including checked/indeterminate state based on given selected items (selectedTopics)
 * @param items - all found items
 * @param selectedItems - all selected items
 * @param allItems - all items (all topics)
 */
export const getSearchTopicsItems = <T extends SearchTopicsItem>(items: T[], selectedItems: T[], allItems: T[]) =>
    items.map(foundItem => {
        const foundItemAndAllChildren = getFlattenNodes(foundItem.children)
        const isChecked = (foundItemAndAllChildren.length &&
            foundItemAndAllChildren.every(child => selectedItems.some(selectedTopic => child.id === selectedTopic.id))) ||
            selectedItems.some(selectedTopic => foundItem.id === selectedTopic.id);
        const isIndeterminate = !isChecked &&
            foundItemAndAllChildren.length &&
            foundItemAndAllChildren.some(child => selectedItems.some(selectedTopic => child.id === selectedTopic.id));
        const AncestorsAndMe = getAncestorsAndMe(foundItem, allItems);
        
        return {
            id: foundItem.id,
            name: foundItem.name,
            displayName: AncestorsAndMe.map(item => item.name).join(' - '),
            children: foundItem.children,
            isChecked,
            isIndeterminate
        }
    });

export const findMatchedTopics = (searchedText: string, topics: Topic[]) => topics.filter(topic => topic.name.match(new RegExp(searchedText, 'i')))
