import { Topic } from '../apimodels/TopicsStore';

export interface INode {
    children: INode[];
    id: number;
}

export interface IOrderableNode extends INode {
    uiOrder: number;
}

export const getLeaves = <T extends INode>(item: T) => {
    let { children } = item;
    let result = children.filter(i => i.children.length === 0);

    while (children.length > 0) {
        children = children.map(child => child.children).reduce((a, b) => a.concat(b));
        result = result.concat(children.filter(i => i.children.length === 0));
    }

    return result as T[];
}

export const getAncestorsAndMe = <T extends INode>(leaf: T, allItems: T[]) => {
    const result: T[] = [leaf];
    let itemsToSearch: T[] = allItems;
    let ancestorWasFound: boolean;

    // loop until we don't reach the top level ancestor
    while(!allItems.some(item => item.id === result[0].id) && itemsToSearch.length > 0) {
        ancestorWasFound = false;

        // loop through items on the same level
        itemsToSearch.every(item => {
            // particular item has a child what we are searching for
            if(item.children.some(child => child.id === result[0].id)) {
                result.unshift(item);
                itemsToSearch = allItems;
                ancestorWasFound = true;
                return false;
            }
            return true;
        });

        // we didn't find the leaf so we are going to loop through deeper level in the next iteration
        if(!ancestorWasFound) {
            itemsToSearch = itemsToSearch
              .map(item => item.children)
              .reduce((a, b) => a.concat(b), []) as T[];
        }
    }

    return result;
}

export const flatSort = (a: IOrderableNode, b: IOrderableNode, collection: IOrderableNode[]) => {
    const ancestorsAndMeForA = getAncestorsAndMe(a, collection);
    const ancestorsAndMeForB = getAncestorsAndMe(b, collection);
    const ancestorsAndMeMaxLength = Math.max(ancestorsAndMeForA.length, ancestorsAndMeForB.length);

    for (let i = 0; i < ancestorsAndMeMaxLength; i++) {
        const nodeA = ancestorsAndMeForA[i];
        const nodeB = ancestorsAndMeForB[i];

        if (nodeA && nodeB) {
            if (nodeA.uiOrder === nodeB.uiOrder) {
                continue;
            } else {
                return nodeA.uiOrder - nodeB.uiOrder;
            }
        } else if (nodeA) {
            return -1;
        } else { // nodeB
            return 1;
        }
    }
}

// returns all nodes in hierarchy flatten in single array
export const getFlattenNodes = <T extends INode>(items: T[]) => {
    if(!items.length) {
        return [];
    }

    let children: T[] = items
        .map(item => item.children)
        .reduce((a, b) => a.concat(b)) as T[];
    let result: T[] = children.concat(items)

    while (children.length > 0) {
        children = children
            .map(item => item.children)
            .reduce((a, b) => a.concat(b)) as T[];
        result = result.concat(children);
    }

    return result;
}